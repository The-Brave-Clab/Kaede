import subprocess
import os
import platform
import pathlib
import difflib
import uuid
import shutil
import sys
import boto3
import botocore

os.environ['AWS_SHARED_CREDENTIALS_FILE'] = str(pathlib.Path(__file__).parent.absolute() / "aws_credentials")
session = boto3.Session(profile_name='scenario_patcher')
credentials = session.get_credentials()

access_key = credentials.access_key
secret_key = credentials.secret_key

s3 = boto3.resource('s3', aws_access_key_id=credentials.access_key, aws_secret_access_key=credentials.secret_key)
PATCH_BUCKET = 'yuyuyui-scenario-patch'
EXTRACTED_BUCKET = 'yuyuyui-datamine-extracted'
patch_bucket = s3.Bucket(PATCH_BUCKET)
extracted_bucket = s3.Bucket(EXTRACTED_BUCKET)

def ensure_folder(folder : pathlib.Path) -> pathlib.Path:
    if not folder.is_dir():
        os.makedirs(str(folder), exist_ok=True)
    return folder

def get_temp_file(path : str) -> pathlib.Path:
    return ensure_folder(pathlib.Path(path).absolute()) / f"temp_{uuid.uuid4()}.txt"

def open_file_and_wait(filepath : pathlib.Path):
    if platform.system() == 'Darwin':       # macOS
        subprocess.call(('open', str(filepath)))
    elif platform.system() == 'Windows':    # Windows
        subprocess.call(('notepad.exe', str(filepath)))
    else:                                   # linux variants
        subprocess.call(('xdg-open', str(filepath)))

def create_diff(old_file: pathlib.Path, new_file: pathlib.Path):
    with open(old_file, "r", encoding='utf-8') as f:
        file_1 = f.readlines()
    with open(new_file, "r", encoding='utf-8') as f:
        file_2 = f.readlines()

    if file_1 == file_2:
        return []

    delta = difflib.unified_diff(file_1, file_2, old_file.name, new_file.name)
    return delta

if __name__ == "__main__":
    script_name = sys.argv[1]
    temp_path = sys.argv[2]

    original_file = get_temp_file(temp_path)
    temp_file = get_temp_file(temp_path)

    script_types = ["alias", "ignore", "script"]

    for script_type in script_types:
        input_key = f"assets/android/_yuyuyuassetbundles/resources/adv/{script_name}/{script_name}_{script_type}.txt"
        input_obj = extracted_bucket.Object(input_key)
        try:
            input_obj_response = input_obj.get()
        except botocore.exceptions.ClientError as e:
            print(f"[Not Found] {script_name}_{script_type}.txt")
            continue
        input_obj_bytes = input_obj_response["Body"].read()
        original_file.write_bytes(input_obj_bytes)

        shutil.copy(str(original_file), str(temp_file))

        open_file_and_wait(temp_file)

        diff = create_diff(original_file, temp_file)

        if diff != []:
            diff_file = get_temp_file(temp_path)
            with open(str(diff_file), "w", encoding="utf-8") as f:
                f.writelines(diff)

            diff_content = diff_file.read_bytes()
            output_key = f"adv/{script_name}/{script_name}_{script_type}.txt"
            patch_obj = patch_bucket.Object(output_key)
            patch_obj.put(Body=diff_content, ContentType='text/plain; charset=utf-8')
            print(str(diff_file))
