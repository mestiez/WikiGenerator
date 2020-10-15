metadata
title Metadata
order 1
/metadata

Most objects in WikiGenerator can have metadata defined for themselves. This page will explain what metadata exists and how to define them.

## Wiki metadata
The `wiki.json` file describes some information about the wiki itself.
```json
{
  "Title": "WikiGenerator",
  "Author": "zooi",
  "MaxSidebarEntries": 25,
  "Mirror": [
    "media"
  ]
}
```
*The wiki.json file for this wiki*

The `title` is what the wiki thinks its name is. The `author` is who it thinks made it. `MaxSidebarEntries` determines the maximum amount of links per category in the sidebar. The `mirror` array tells the generator what extra files to copy to the output folder, which is useful for things like images.

This file **must** exist for the generator to work.

## Page metadata

Every page needs to have metadata defined for itself. Page metadata is added by having this at the top of every Markdown file:
```properties
metadata
title Title goes here
order 0
description Some description
/metadata
```
The `title` field is necessary, the rest is optional. The `title` field will create a header on every page and tells the generator what to display this file as. The `order` field determines the order of this article in any list it is generated in. The `description` field is currently only used for category metadata which will be described next.

## Category metadata

Categories are just folders with Markdown files in them. This Markdown file resides in a folder called `advancedUsage`. This is not what the category is called, though. This is because of the `metadata.json` file that also lives in the folder. This file is optional and can describe the same information as the "Page metadata" section. 

The `description` field is unique to categories, and will replace the text that appears on the category page, which is *"Here follow all pages under this category."* by default.

The metadata for the "Advanced Usage" category looks like this:
```json
{
    "title": "Advanced Usage"
}
```