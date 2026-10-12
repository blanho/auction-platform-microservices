import json
from pathlib import Path
import subprocess
import sys
import unittest


class SystemComposeIsolationTests(unittest.TestCase):
    def prepare(self, config):
        result = subprocess.run(
            [sys.executable, str(Path(__file__).parents[1] / "prepare-system-compose.py")],
            input=json.dumps(config), text=True, capture_output=True, check=True,
        )
        return json.loads(result.stdout)

    def test_resources_are_scoped_to_the_disposable_project(self):
        prepared = self.prepare({
            "name": "auction-platform",
            "volumes": {"data": {"name": "auction-data", "driver": "local"}},
            "networks": {"default": {"name": "auction-network", "driver": "bridge"}},
            "services": {"api": {
                "container_name": "auction-api", "restart": "always",
                "volumes": ["data:/data"], "environment": {"SETTING": "value"},
            }},
        })

        self.assertEqual(prepared["name"], "auction-system-validation")
        self.assertEqual(prepared["volumes"], {"data": {"driver": "local"}})
        self.assertEqual(prepared["networks"], {"default": {"driver": "bridge"}})
        api = prepared["services"]["api"]
        self.assertNotIn("container_name", api)
        self.assertEqual(api["restart"], "no")
        self.assertEqual(api["volumes"], ["data:/data"])
        self.assertEqual(api["environment"], {"SETTING": "value"})

    def test_ports_bind_loopback_and_dependencies_avoid_normal_stack_ports(self):
        ports = [
            {"published": str(port), "target": port, "host_ip": "0.0.0.0", "protocol": "tcp"}
            for port in (3000, 6001, 5432, 6379, 5672)
        ]

        prepared = self.prepare({"services": {"stack": {"ports": ports}}})
        isolated_ports = prepared["services"]["stack"]["ports"]

        self.assertEqual([p["published"] for p in isolated_ports],
                         ["3000", "6001", "25432", "26379", "25672"])
        self.assertTrue(all(p["host_ip"] == "127.0.0.1" for p in isolated_ports))
        self.assertEqual([p["target"] for p in isolated_ports], [p["target"] for p in ports])
        self.assertTrue(all(p["protocol"] == "tcp" for p in isolated_ports))

    def test_health_checks_keep_the_probe_with_bounded_startup_retries(self):
        probe = ["CMD", "check-health"]
        prepared = self.prepare({"services": {
            "api": {"healthcheck": {"test": probe, "interval": "1m", "retries": 100}},
            "worker": {},
        }})

        self.assertEqual(prepared["services"]["api"]["healthcheck"], {
            "test": probe, "interval": "5s", "timeout": "10s", "retries": 30,
            "start_period": "30s",
        })
        self.assertNotIn("healthcheck", prepared["services"]["worker"])


if __name__ == "__main__":
    unittest.main()
