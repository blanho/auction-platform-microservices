"""Reject missing, empty, failed or skipped test runs; includes required integration tests."""
import sys
from pathlib import Path
import xml.etree.ElementTree as ET


def verify(root, projects):
    errors = []
    for project in projects:
        name = Path(project).stem
        report = Path(root) / name / f"{name}.trx"
        try:
            tree = ET.parse(report)
            counters = tree.find(".//{*}Counters")
            if counters is None:
                raise ValueError("missing counters")
            total = int(counters.get("total", "0"))
            passed = int(counters.get("passed", "0"))
            if total == 0 or passed != total:
                errors.append(f"{name}: all tests must pass and execute: {counters.attrib}")
            else:
                print(f"{name}: {passed} executed and passed; zero skipped")
        except (OSError, ET.ParseError, ValueError) as error:
            errors.append(f"{name}: invalid or missing report: {error}")
    return errors


if __name__ == "__main__":
    if len(sys.argv) < 3:
        sys.exit("Usage: verify-test-results.py RESULTS_ROOT PROJECT [PROJECT ...]")
    failures = verify(sys.argv[1], sys.argv[2:])
    if failures:
        sys.exit("\n".join(failures))
