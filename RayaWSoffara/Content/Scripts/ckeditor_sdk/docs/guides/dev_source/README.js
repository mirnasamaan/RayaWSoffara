Ext.data.JsonP.dev_source({"guide":"<!--\nCopyright (c) 2003-2015, CKSource - Frederico Knabben. All rights reserved.\nFor licensing, see LICENSE.md.\n-->\n\n\n<h1 id='dev_source-section-getting-the-source-code'>Getting the Source Code</h1>\n<div class='toc'>\n<p><strong>Contents</strong></p>\n<ol>\n<li><a href='#!/guide/dev_source-section-cloning-from-github'>Cloning from GitHub</a></li>\n<li>\n<a href='#!/guide/dev_source-section-performance'>Performance</a></li></ol>\n</div>\n\n<p>Working with the source code of CKEditor may be useful. These are some possible situations that you may face:</p>\n\n<ul>\n<li>You are developing plugins or skins, so you want to build your own distributions.</li>\n<li>You are assembling the editor by yourself, by adding plugins and skins to it manually.</li>\n<li>You want to understand the CKEditor API better by reading the code.</li>\n<li>You want to fix an issue. (Yes, do it!)</li>\n<li>You want to propose some new features or enhancements. (Likewise, we are <a href=\"https://github.com/ckeditor/ckeditor-dev/pulls\">looking forward to them</a>!)</li>\n</ul>\n\n\n<h2 id='dev_source-section-cloning-from-github'>Cloning from GitHub</h2>\n\n<p>The CKEditor source code is available in the <a href=\"https://github.com/ckeditor/ckeditor-dev\">ckeditor-dev</a> Git repository hosted at GitHub.</p>\n\n<p>If you have Git installed in your system, use the following command line call to create your local copy:</p>\n\n<pre><code>git clone https://github.com/ckeditor/ckeditor-dev.git\n</code></pre>\n\n<p>This will download the full CKEditor development code into the <code>ckeditor-dev</code> folder.</p>\n\n<h2 id='dev_source-section-performance'>Performance</h2>\n\n<p>Note that the source code version of CKEditor is not optimized for production websites. It works flawlessly on a local computer or network, but if you include it in your production website, it may need to do more than two hundred HTTP requests to download more than a megabyte of code.</p>\n\n<p>Because of this <strong>do not use the source code version of CKEditor in production websites</strong>!</p>\n\n<p>Once your local development is completed, <a href=\"#!/guide/dev_build\">create a CKEditor build</a> that will be distribution-ready.</p>\n","title":"Getting Source Code","meta_description":"How to get CKEditor source code.","meta_keywords":"ckeditor, editor, source, code, github, repository, cloning, git"});