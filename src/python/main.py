# This script doesn't require Python installation - it will use the embedded Python
import sys
import os

def main():
    print("Hello from Python!")
    print(f"Running from: {os.path.abspath(os.path.dirname(__file__))}")

if __name__ == "__main__":
    main()