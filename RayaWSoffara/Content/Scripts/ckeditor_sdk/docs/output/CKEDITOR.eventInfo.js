Ext.data.JsonP.CKEDITOR_eventInfo({"tagname":"class","name":"CKEDITOR.eventInfo","autodetected":{},"files":[{"filename":"eventInfo.js","href":"eventInfo.html#CKEDITOR-eventInfo"}],"abstract":true,"members":[{"name":"data","tagname":"property","owner":"CKEDITOR.eventInfo","id":"property-data","meta":{}},{"name":"editor","tagname":"property","owner":"CKEDITOR.eventInfo","id":"property-editor","meta":{}},{"name":"listenerData","tagname":"property","owner":"CKEDITOR.eventInfo","id":"property-listenerData","meta":{}},{"name":"name","tagname":"property","owner":"CKEDITOR.eventInfo","id":"property-name","meta":{}},{"name":"sender","tagname":"property","owner":"CKEDITOR.eventInfo","id":"property-sender","meta":{}},{"name":"cancel","tagname":"method","owner":"CKEDITOR.eventInfo","id":"method-cancel","meta":{}},{"name":"removeListener","tagname":"method","owner":"CKEDITOR.eventInfo","id":"method-removeListener","meta":{}},{"name":"stop","tagname":"method","owner":"CKEDITOR.eventInfo","id":"method-stop","meta":{}}],"alternateClassNames":[],"aliases":{},"id":"class-CKEDITOR.eventInfo","short_doc":"Virtual class that illustrates the features of the event object to be\npassed to event listeners by a CKEDITOR.event b...","component":false,"superclasses":[],"subclasses":[],"mixedInto":[],"mixins":[],"parentMixins":[],"requires":[],"uses":[],"html":"<div><pre class=\"hierarchy\"><h4>Files</h4><div class='dependency'><a href='source/eventInfo.html#CKEDITOR-eventInfo' target='_blank'>eventInfo.js</a></div></pre><div class='doc-contents'><p>Virtual class that illustrates the features of the event object to be\npassed to event listeners by a <a href=\"#!/api/CKEDITOR.event\" rel=\"CKEDITOR.event\" class=\"docClass\">CKEDITOR.event</a> based object.</p>\n\n<p>This class is not really part of the API.</p>\n</div><div class='members'><div class='members-section'><div class='definedBy'>Defined By</div><h3 class='members-title icon-property'>Properties</h3><div class='subsection'><div id='property-data' class='member first-child not-inherited'><a href='#' class='side expandable'><span>&nbsp;</span></a><div class='title'><div class='meta'><span class='defined-in' rel='CKEDITOR.eventInfo'>CKEDITOR.eventInfo</span><br/><a href='source/eventInfo.html#CKEDITOR-eventInfo-property-data' target='_blank' class='view-source'>view source</a></div><a href='#!/api/CKEDITOR.eventInfo-property-data' class='name expandable'>data</a> : Object<span class=\"signature\"></span></div><div class='description'><div class='short'>Any kind of additional data. ...</div><div class='long'><p>Any kind of additional data. Its format and usage is event dependent.</p>\n\n<pre><code>someObject.on( 'someEvent', function( event ) {\n    alert( event.data ); // 'Example'\n} );\nsomeObject.fire( 'someEvent', 'Example' );\n</code></pre>\n</div></div></div><div id='property-editor' class='member  not-inherited'><a href='#' class='side expandable'><span>&nbsp;</span></a><div class='title'><div class='meta'><span class='defined-in' rel='CKEDITOR.eventInfo'>CKEDITOR.eventInfo</span><br/><a href='source/eventInfo.html#CKEDITOR-eventInfo-property-editor' target='_blank' class='view-source'>view source</a></div><a href='#!/api/CKEDITOR.eventInfo-property-editor' class='name expandable'>editor</a> : <a href=\"#!/api/CKEDITOR.editor\" rel=\"CKEDITOR.editor\" class=\"docClass\">CKEDITOR.editor</a><span class=\"signature\"></span></div><div class='description'><div class='short'>The editor instance that holds the sender. ...</div><div class='long'><p>The editor instance that holds the sender. May be the same as sender. May be\nnull if the sender is not part of an editor instance, like a component\nrunning in standalone mode.</p>\n\n<pre><code>myButton.on( 'someEvent', function( event ) {\n    alert( event.editor == myEditor ); // true\n} );\nmyButton.fire( 'someEvent', null, myEditor );\n</code></pre>\n</div></div></div><div id='property-listenerData' class='member  not-inherited'><a href='#' class='side expandable'><span>&nbsp;</span></a><div class='title'><div class='meta'><span class='defined-in' rel='CKEDITOR.eventInfo'>CKEDITOR.eventInfo</span><br/><a href='source/eventInfo.html#CKEDITOR-eventInfo-property-listenerData' target='_blank' class='view-source'>view source</a></div><a href='#!/api/CKEDITOR.eventInfo-property-listenerData' class='name expandable'>listenerData</a> : Object<span class=\"signature\"></span></div><div class='description'><div class='short'>Any extra data appended during the listener registration. ...</div><div class='long'><p>Any extra data appended during the listener registration.</p>\n\n<pre><code>someObject.on( 'someEvent', function( event ) {\n    alert( event.listenerData ); // 'Example'\n}, null, 'Example' );\n</code></pre>\n</div></div></div><div id='property-name' class='member  not-inherited'><a href='#' class='side expandable'><span>&nbsp;</span></a><div class='title'><div class='meta'><span class='defined-in' rel='CKEDITOR.eventInfo'>CKEDITOR.eventInfo</span><br/><a href='source/eventInfo.html#CKEDITOR-eventInfo-property-name' target='_blank' class='view-source'>view source</a></div><a href='#!/api/CKEDITOR.eventInfo-property-name' class='name expandable'>name</a> : String<span class=\"signature\"></span></div><div class='description'><div class='short'>The event name. ...</div><div class='long'><p>The event name.</p>\n\n<pre><code>someObject.on( 'someEvent', function( event ) {\n    alert( event.name ); // 'someEvent'\n} );\nsomeObject.fire( 'someEvent' );\n</code></pre>\n</div></div></div><div id='property-sender' class='member  not-inherited'><a href='#' class='side expandable'><span>&nbsp;</span></a><div class='title'><div class='meta'><span class='defined-in' rel='CKEDITOR.eventInfo'>CKEDITOR.eventInfo</span><br/><a href='source/eventInfo.html#CKEDITOR-eventInfo-property-sender' target='_blank' class='view-source'>view source</a></div><a href='#!/api/CKEDITOR.eventInfo-property-sender' class='name expandable'>sender</a> : Object<span class=\"signature\"></span></div><div class='description'><div class='short'>The object that publishes (sends) the event. ...</div><div class='long'><p>The object that publishes (sends) the event.</p>\n\n<pre><code>someObject.on( 'someEvent', function( event ) {\n    alert( event.sender == someObject ); // true\n} );\nsomeObject.fire( 'someEvent' );\n</code></pre>\n</div></div></div></div></div><div class='members-section'><div class='definedBy'>Defined By</div><h3 class='members-title icon-method'>Methods</h3><div class='subsection'><div id='method-cancel' class='member first-child not-inherited'><a href='#' class='side expandable'><span>&nbsp;</span></a><div class='title'><div class='meta'><span class='defined-in' rel='CKEDITOR.eventInfo'>CKEDITOR.eventInfo</span><br/><a href='source/eventInfo.html#CKEDITOR-eventInfo-method-cancel' target='_blank' class='view-source'>view source</a></div><a href='#!/api/CKEDITOR.eventInfo-method-cancel' class='name expandable'>cancel</a>( <span class='pre'></span> )<span class=\"signature\"></span></div><div class='description'><div class='short'>Indicates that the event is to be cancelled (if cancelable). ...</div><div class='long'><p>Indicates that the event is to be cancelled (if cancelable).</p>\n\n<pre><code>someObject.on( 'someEvent', function( event ) {\n    event.cancel();\n} );\nsomeObject.on( 'someEvent', function( event ) {\n    // This one will not be called.\n} );\nalert( someObject.fire( 'someEvent' ) ); // false\n</code></pre>\n</div></div></div><div id='method-removeListener' class='member  not-inherited'><a href='#' class='side expandable'><span>&nbsp;</span></a><div class='title'><div class='meta'><span class='defined-in' rel='CKEDITOR.eventInfo'>CKEDITOR.eventInfo</span><br/><a href='source/eventInfo.html#CKEDITOR-eventInfo-method-removeListener' target='_blank' class='view-source'>view source</a></div><a href='#!/api/CKEDITOR.eventInfo-method-removeListener' class='name expandable'>removeListener</a>( <span class='pre'></span> )<span class=\"signature\"></span></div><div class='description'><div class='short'>Removes the current listener. ...</div><div class='long'><p>Removes the current listener.</p>\n\n<pre><code>someObject.on( 'someEvent', function( event ) {\n    event.removeListener();\n    // Now this function won't be called again by 'someEvent'.\n} );\n</code></pre>\n</div></div></div><div id='method-stop' class='member  not-inherited'><a href='#' class='side expandable'><span>&nbsp;</span></a><div class='title'><div class='meta'><span class='defined-in' rel='CKEDITOR.eventInfo'>CKEDITOR.eventInfo</span><br/><a href='source/eventInfo.html#CKEDITOR-eventInfo-method-stop' target='_blank' class='view-source'>view source</a></div><a href='#!/api/CKEDITOR.eventInfo-method-stop' class='name expandable'>stop</a>( <span class='pre'></span> )<span class=\"signature\"></span></div><div class='description'><div class='short'>Indicates that no further listeners are to be called. ...</div><div class='long'><p>Indicates that no further listeners are to be called.</p>\n\n<pre><code>someObject.on( 'someEvent', function( event ) {\n    event.stop();\n} );\nsomeObject.on( 'someEvent', function( event ) {\n    // This one will not be called.\n} );\nalert( someObject.fire( 'someEvent' ) ); // true\n</code></pre>\n</div></div></div></div></div></div></div>","meta":{"abstract":true}});