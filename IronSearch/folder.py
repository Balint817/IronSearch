import os

def print_tree(start_path, prefix=""):
    items = sorted(os.listdir(start_path))
    for i, name in enumerate(items):
        if name == "folder.py" or name == "bin" or name == "obj":
            continue
        path = os.path.join(start_path, name)
        connector = "└── " if i == len(items) - 1 else "├── "
        print(prefix + connector + name)

        if os.path.isdir(path):
            extension = "    " if i == len(items) - 1 else "│   "
            print_tree(path, prefix + extension)

if __name__ == "__main__":
    root = "."  # change this to your target directory
    print(root)
    print_tree(root)