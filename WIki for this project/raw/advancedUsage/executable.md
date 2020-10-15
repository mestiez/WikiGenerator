metadata
title Executable
order 0
/metadata

WikiGenerator.exe takes in 0 or 2 arguments. Its behaviour depends on the amount of arguments that are given.

## Initialisation mode

When you call WikiGenerator in a folder without any arguments, it will initialise that folder as a wiki project. This will create the default folder structure as described in [Basic Usage](./usage.html). This is the recommended way to initialise a project because it ensures the correct conditions.

## Build mode

Otherwise, the program expects exactly two arguments: an input and output location. The output location is expected to be empty but it will overwrite any files that are inside anyway. The input location is expected to have a [`wiki.json`](./advancedUsage/metadata.html) file. 