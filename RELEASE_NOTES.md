#### 0.1.2 - 05/04/2021

- Add model fallback path if they can not be loaded from the assembly

#### 0.1.1 - 05/04/2021

- Fix models not being present in the tool package

#### 0.1.0 - 04/29/2021

First release of the imlp cli tool. 

- all basic cli commands:
   - `-s` to predict for single sequences, `-f` to predict for all proteins in a fasta file
   - `-o` to write to a file instead of stdout
   - `-m` to select between plant and nonplant model
   - `-p` to add a directory to save plots to
   - `-pr` to pass a regex for protein names
   - `-v` to controll verbosity of logging
   See https://github.com/CSBiology/iMLP#usage for more info

- dotnet tool for easier installation and usage (See https://github.com/CSBiology/iMLP#dotnet-tool for more)
- Dockerfile for a containerized, linux based app with all binary native dependencies (See https://github.com/CSBiology/iMLP#docker for more)
- binaries for linux and windows (See https://github.com/CSBiology/iMLP#published-binaries for more)