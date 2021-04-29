#### 0.1.0 - 04/29/2021

First release of the imlp cli tool. 

- all basic cli commands:
   - `-s` to predict for single sequences, `-f` to predict for all proteins in a fasta file
   - `-o` to write to a file instead of stdout
   - `-p` to add a directory to save plots to
   - `-pr` to pass a regex for protein names
   - `-v` to controll verbosity of logging
   See https://github.com/CSBiology/iMLP/blob/main/README.md#usage for more info

- dotnet tool for easier installation and usage (See for more)
- Dockerfile for a containerized, linux based app (See for more)
- binaries for linux and windows (See for more)