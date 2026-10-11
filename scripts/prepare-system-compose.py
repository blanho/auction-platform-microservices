#!/usr/bin/env python3
"""Isolate a resolved Compose config for disposable, local system validation."""
import json
import sys

config = json.load(sys.stdin)
config["name"] = "auction-system-validation"
for section in ("volumes", "networks"):
    for definition in config.get(section, {}).values():
        definition.pop("name", None)
for service in config["services"].values():
    service.pop("container_name", None)
    service["restart"] = "no"
    for port in service.get("ports", []):
        published = int(port["published"])
        port["host_ip"] = "127.0.0.1"
        port["published"] = str(published if published in (3000, 6001) else published + 20000)
    if "healthcheck" in service:
        service["healthcheck"].update(interval="5s", timeout="10s", retries=30, start_period="30s")
json.dump(config, sys.stdout, indent=2)
