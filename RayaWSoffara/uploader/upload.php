<?php

header('Content-Type: application/json');
//ini_set('memory_limit','16M');

$error					= false;

$absolutedir			= dirname(__FILE__);
$dir					= '/tmp/';
$serverdir				= $absolutedir.$dir;

$imagequality			= 100;
$imagefunction			= "imagejpeg";

$tmp					= explode(',',$_POST['data']);
$imgdata 				= base64_decode($tmp[1]);

//$filename				= substr($_POST['name'],0,-(strlen(end(explode('.',$_POST['name']))) + 1)).'.'.substr(sha1(time()),0,6).'.'.$extension;

$info					= getimagesizefromstring($imgdata); 
$im 					= imagecreatefromstring($imgdata);
//var_dump($info);
//exit();

switch(strtolower($info['mime'])) {
	case 'image/png':
		$extension		= "png";
		$imagefunction	= "imagepng";
		$imagequality	= 0;
	break;
	case 'image/jpeg':
		$extension		= "jpg";
	break;
	case 'image/gif':
		$extension		= "gif";
		$imagefunction	= "imagegif";
	break;
	default:
		$error			= "Mimetype not supported"; 
	break;
}

$resizedImage 			= imagecreatetruecolor($_POST['imageWidth'], $_POST['imageHeight']);
imagecopyresampled($resizedImage, $im, 0, 0, 0, 0, $_POST['imageWidth'], $_POST['imageHeight'], $info[0], $info[1]);

$finalImage 			= imagecreatetruecolor($_POST['width'], $_POST['height']);
imagecopyresampled($finalImage, $resizedImage, 0, 0, $_POST['left'], $_POST['top'], $_POST['width'], $_POST['height'], $_POST['width'], $_POST['height']);

$filename				= substr($_POST['name'],0,-(strlen(end(explode('.',$_POST['name']))) + 1)).'.'.substr(sha1(time()),0,6).'.'.$extension;

if ($extension == 'png') {
	imagesavealpha($finalImage, true);
}
$imagefunction($finalImage, $serverdir.$filename, $imagequality);


$response = array(
		"status" 		=> "success",
		"url" 			=> $dir.$filename.'?'.time(), //added the time to force update when editting multiple times
		"filename" 		=> $filename
);


if (!empty($_POST['saveOriginal'])) {
	$original			= substr($_POST['name'],0,-(strlen(end(explode('.',$_POST['name']))) + 1)).'.'.substr(sha1(time()),0,6).'.original.'.$extension;

	$handle				= fopen($serverdir.$original,'w');
	fwrite($handle, $imgdata);
	fclose($handle);

	$response['original']	= $original;
}


print json_encode($response);
