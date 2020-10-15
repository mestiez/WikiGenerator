metadata
title Basic Usage
/metadata

This page will explain how to use the application in the most basic way.

## Installation

Extract the archive into its own folder and [add that folder to your PATH variable](https://www.architectryan.com/2018/03/17/add-to-the-path-on-windows-10/). 
You can now call `WikiGenerator` as a command everywhere.

## Project setup

To create a wiki project, simply run `WikiGenerator` in your target folder. This will create the default folder structure.

### Folder structure

The default folder structure looks like this:
```yaml
 - build # output folder (generated HTML files)
 - inject # injection source folder
    - page.html
 - raw # input folder (your MD files)
    - index.md # index file, the wiki homepage
    - wiki.json # metadata file containing wiki information
 - build.bat # the build script
```
`build.bat` will run `WikiGenerator raw build`. which builds the contents in `raw` to `build`. That should have been really obvious. 

## Building and serving the wiki

Building the wiki is as simple as running the batch file. You can also call `WikiGenrator raw build` manually.

To serve the wiki, just run the `build` folder as a local server. I suggest using [reload](https://www.npmjs.com/package/reload). Just run `npm i -g reload` and then run `reload` in the output folder. You can then navigate to [localhost:8080](http://localhost:8080/) to view your website.