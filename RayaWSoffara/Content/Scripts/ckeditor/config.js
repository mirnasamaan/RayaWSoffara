/**
 * @license Copyright (c) 2003-2015, CKSource - Frederico Knabben. All rights reserved.
 * For licensing, see LICENSE.md or http://ckeditor.com/license
 */

CKEDITOR.editorConfig = function( config ) {
	// Define changes to default configuration here. For example:
	// config.language = 'fr';
    // config.uiColor = '#AADC6E';
    config.extraPlugins = 'button,toolbar,notification,notificationaggregator,filetools,uploadwidget,widget,lineutils,uploadimage';
    config.uploadUrl = '/Article/UploadImage';
    //config.baseUrl = '/Content/Article_Images/';
    //config.baseDir = '/Content/Article_Images/';
};

//CKEDITOR.editorConfig = function (config) {
//    config.extraPlugins = 'button,toolbar,notification,notificationaggregator,filetools,uploadwidget,imgupload';
//    //config.uploadUrl = '/uploader/upload.php';
//};