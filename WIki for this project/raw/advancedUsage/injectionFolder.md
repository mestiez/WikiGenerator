metadata
title Injection folder
order 2
/metadata

The generator bases all generated HTML on whats in the `inject` folder in your project. Currently, this only contains a `page.html`. You have full control over this file and can completely change where content is injected and what everything looks like.

You may notice the keys like `#MARKDOWN_RESULT` inside the HTML file. This is where the generator puts the generated HTML. You can move these around, delete them completely, or even have multiple of them. Here follow all the keys understood by the generator:

![Injection keys](./media/keys.png)

*This had to be an image because the generator would replace the examples lmao*

If you understand HTML and CSS, this file is where you can customise the wiki.

## Important notice
If the build files aren't hosted at the root of your server, it's wise to add the appropriate [`<base>`](https://developer.mozilla.org/en-US/docs/Web/HTML/Element/base) tag to the `page.html` to ensure links point to the correct address.