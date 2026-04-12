import os
import shutil

def clean():
    '''
    清理指定目录和文件
    '''
    del_dirs = ['__pycache__', '.pytest_cache', 'bin', 
                '.vscode', 'build', 'gcm.cache']
    del_files = ['db.sqlite3', 'demo']
    del_exts = ['.obj', ]
    for root, dirs, files in os.walk('.'):
        for dir in dirs:
            if  dir in del_dirs: 
                shutil.rmtree(os.path.join(root,dir))
        for filename in files:
            if filename in del_files:
                os.system(f'rm -f {os.path.join(root,filename)}')
            if os.path.splitext(filename)[1] in del_exts:
                os.system(f'rm -f {os.path.join(root,filename)}')   


if __name__ == '__main__':
    clean()
    