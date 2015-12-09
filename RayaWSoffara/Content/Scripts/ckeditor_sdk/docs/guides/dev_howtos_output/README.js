Ext.data.JsonP.dev_howtos_output({"guide":"<!--\nCopyright (c) 2003-2015, CKSource - Frederico Knabben. All rights reserved.\nFor licensing, see LICENSE.md.\n-->\n\n\n<h1 id='dev_howtos_output-section-output'>Output</h1>\n<div class='toc'>\n<p><strong>Contents</strong></p>\n<ol>\n<li><a href='#!/guide/dev_howtos_output-section-how-do-i-output-html-instead-of-xhtml-code-using-ckeditor%3F'>How Do I Output HTML Instead of XHTML Code Using CKEditor?</a></li>\n<li>\n<a href='#!/guide/dev_howtos_output-section-how-do-i-output-bbcode-instead-of-html-code-using-ckeditor%3F'>How Do I Output BBCode Instead of HTML Code Using CKEditor?</a></li></ol>\n</div>\n\n<h2 id='dev_howtos_output-section-how-do-i-output-html-instead-of-xhtml-code-using-ckeditor%3F'>How Do I Output HTML Instead of XHTML Code Using CKEditor?</h2>\n\n<p>If you want CKEditor to output valid HTML4 code instead of XHTML, you should configure the behavior of the <a href=\"#!/api/CKEDITOR.dataProcessor\" rel=\"CKEDITOR.dataProcessor\" class=\"docClass\">dataProcessor</a>.</p>\n\n<p>For some tips on how to achieve this, check the <a href=\"#!/guide/dev_output_format\">Output Formatting</a> section of <a href=\"#!/guide/dev\">Developer's Guide</a> as well as the Output HTML (<code>plugins/htmlwriter/samples/outputhtml.html</code>) and Output XHTML (<code>samples/xhtmlstyle.html</code>) samples that can be found in CKEditor installation package.</p>\n\n<p>If, for example, you want CKEditor to output the self-closing tags in the HTML4 way, creating <code>&lt;br&gt;</code> elements instead of <code>&lt;br /&gt;</code>, configure the <code>selfClosingEnd</code> setting in the following way.</p>\n\n<pre><code>CKEDITOR.on( 'instanceReady', function( ev ) {\n    ev.editor.dataProcessor.writer.selfClosingEnd = '&gt;';\n});\n</code></pre>\n\n<h2 id='dev_howtos_output-section-how-do-i-output-bbcode-instead-of-html-code-using-ckeditor%3F'>How Do I Output BBCode Instead of HTML Code Using CKEditor?</h2>\n\n<p>You should try the <a href=\"http://ckeditor.com/addon/bbcode\">BBCode plugin</a>.</p>\n","title":"Output","meta_description":"Most frequently asked question and answers about CKEditor output.","meta_keywords":"ckeditor, editor, wysiwyg, howto, howtos, faq, questions, answers, html, xhtml, bbcode, output, dataprocessor, code, data, source, formatting"});