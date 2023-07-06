import subprocess
import contextlib
import joblib
import pathlib
import os
import tqdm
import multiprocessing

@contextlib.contextmanager
def tqdm_joblib(tqdm_object):
    """Context manager to patch joblib to report into tqdm progress bar given as argument"""

    class TqdmBatchCompletionCallback(joblib.parallel.BatchCompletionCallBack):
        def __init__(self, *args, **kwargs):
            super().__init__(*args, **kwargs)

        def __call__(self, *args, **kwargs):
            tqdm_object.update(n=self.batch_size)
            return super().__call__(*args, **kwargs)

    old_batch_callback = joblib.parallel.BatchCompletionCallBack
    joblib.parallel.BatchCompletionCallBack = TqdmBatchCompletionCallback
    try:
        yield tqdm_object
    finally:
        joblib.parallel.BatchCompletionCallBack = old_batch_callback
        tqdm_object.close()


def EnsureFolder(folder: pathlib.Path) -> pathlib.Path:
    if not folder.is_dir():
        os.makedirs(str(folder), exist_ok=True)
    return folder

script_file_path = pathlib.Path(__file__).parent
build_path = script_file_path.parent / 'Builds' / 'WindowsReleaseIL2CPP' / 'Kaede.exe'
test_case_path = script_file_path / pathlib.Path('cases.txt')
result_path = script_file_path / pathlib.Path('results.txt')
log_folder = EnsureFolder(script_file_path / pathlib.Path('logs'))

failed_test = []
failed_reason = ["Passed", "BadParameter", "Exception", "NotImplemented"]

def Test(case : str):
    global failed_test

    log_path = log_folder / f'{case}.txt'

    # Run Kaede, mute std output, wait for exit and get return code
    process = subprocess.Popen([str(build_path), '-batchmode', '-nographics', '-logfile', str(log_path), '-test-mode', '-scenario', case], stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
    process.wait()
    return_code = process.returncode

    if return_code == 0:
        log_path.unlink()
    else:
        failed_test.append((case, return_code))

# read lines in test case file and remove empty lines
test_cases = [line.strip() for line in test_case_path.read_text().splitlines() if line.strip()]

with tqdm_joblib(tqdm.tqdm(test_cases)):
    joblib.Parallel(n_jobs=(multiprocessing.cpu_count()), backend="threading")(joblib.delayed(Test)(c) for c in test_cases)

with open(result_path, 'w') as f:
    for case, return_code in failed_test:
        print(f"Test case {case} failed with code {return_code} ({failed_reason[return_code]})")
        f.write(f"{case}\t{return_code}\n")
