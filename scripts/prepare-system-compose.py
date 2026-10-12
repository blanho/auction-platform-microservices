#!/usr/bin/env python3
"""Isolate a resolved Compose config for disposable, local system validation."""
import json
import sys

SYSTEM_TEST_PROJECT = "auction-system-validation"
APPLICATION_PORTS = {3000, 6001}
DEPENDENCY_PORT_OFFSET = 20000


def prepare_system_compose(config):
    config["name"] = SYSTEM_TEST_PROJECT
    for section in ("volumes", "networks"):
        for definition in config.get(section, {}).values():
            definition.pop("name", None)

    for service in config["services"].values():
        service.pop("container_name", None)
        service["restart"] = "no"
        for port in service.get("ports", []):
            published = int(port["published"])
            port["host_ip"] = "127.0.0.1"
            port["published"] = str(
                published if published in APPLICATION_PORTS else published + DEPENDENCY_PORT_OFFSET
            )
        if "healthcheck" in service:
            service["healthcheck"].update(
                interval="5s", timeout="10s", retries=30, start_period="30s"
            )

    return config


if __name__ == "__main__":
    json.dump(prepare_system_compose(json.load(sys.stdin)), sys.stdout, indent=2)
