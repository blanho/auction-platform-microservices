import importlib.util
from pathlib import Path
import tempfile
import unittest

spec = importlib.util.spec_from_file_location("verifier", Path(__file__).parents[1] / "verify-test-results.py")
verifier = importlib.util.module_from_spec(spec)
spec.loader.exec_module(verifier)


class TestResultVerificationTests(unittest.TestCase):
    def test_missing_empty_failed_and_skipped_reports_fail(self):
        with tempfile.TemporaryDirectory() as root:
            self.assertTrue(verifier.verify(root, ["Required.Tests.csproj"]))
            folder = Path(root) / "Required.Tests"
            folder.mkdir()
            for total, passed in [(0, 0), (3, 2), (1, 0)]:
                (folder / "Required.Tests.trx").write_text(
                    f'<TestRun xmlns="urn:test"><ResultSummary><Counters total="{total}" passed="{passed}" />'
                    '</ResultSummary></TestRun>')
                self.assertTrue(verifier.verify(root, ["Required.Tests.csproj"]))

    def test_all_expected_projects_must_execute(self):
        with tempfile.TemporaryDirectory() as root:
            folder = Path(root) / "Required.Tests"
            folder.mkdir()
            (folder / "Required.Tests.trx").write_text(
                '<TestRun xmlns="urn:test"><ResultSummary><Counters total="3" passed="3" />'
                '</ResultSummary></TestRun>')
            self.assertFalse(verifier.verify(root, ["Required.Tests.csproj"]))
            self.assertTrue(verifier.verify(root, ["Required.Tests.csproj", "Other.Tests.csproj"]))


if __name__ == "__main__":
    unittest.main()
