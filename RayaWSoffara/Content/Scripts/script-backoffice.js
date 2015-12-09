var imageCropWidth = 0;
var imageCropHeight = 0;
var cropPointX = 0;
var cropPointY = 0;
var cropFlag = false;
var editFlag = false;
var originalWidth = 0;
var originalHeight = 0;

function delete_btn(e) {
    var media_id = $(e).parent().parent().parent().attr("id");
    $("#" + media_id + " .media_img").attr("src", "");
    if ($("#modal_video_viewport").find("img").length > 0)
    {
        $("#modal_video_viewport img").remove();
        $("#embedded_video_link").val("");
    }
    var id = media_id.split("_");
    $("#video_url_" + id[1]).val("");
    $("#article_picture_path_" + id[1]).val("");
    $("#" + media_id + " .controls").css("display", "none");
    $("#" + media_id + " .media").unbind('mouseenter mouseleave');
    $("#" + media_id + " .media_dropbox").css("display", "block");
    $("#" + media_id + " .media_dropbox").removeClass("hide");
    $("#my-cropped-image").remove();

    $("#" + media_id).hover(
    function () {
        var media_id = $(this).attr("id");
        if ($("#" + media_id + " .media_img").attr("src") == "") {
            $("#" + media_id + " .controls").css("display", "none");
        } else {
            $("#" + media_id + " .controls").css("display", "block");
        }
    });
}

function removeTopX(e) {
    var media_id = $(e).attr("id");
    media_id = media_id.split("_");
    id = media_id[2];
    $(e).parent().parent().remove();
}

function embed_video(e) {
    var media_id = $(e).parent().parent().attr("id");
    $("#video_id").attr("value", media_id);
    $("#videoModal_toggle_btn").click();
}

function embed_pic(e) {
    var media_id = $(e).parent().parent().attr("id");
    //alert(media_id);
    $("#" + media_id + " .article_picture_btn").click();
}

function OnDragEnter(e) {
    //alert("2");
    $("#media_id").val($(this).attr("id"));
    e.stopPropagation();
    e.preventDefault();
}

function OnDragOver(e) {
    //alert("3");
    e.stopPropagation();
    e.preventDefault();
}

function OnDrop(e) {
    editFlag = false;
    e.stopPropagation();
    e.preventDefault();
    originalWidth = e.target.width;
    originalHeight = e.target.height;
    var media_id = $(this).attr("id");
    var data = e.dataTransfer.getData("text/html");
    if ($("#edit_modal_body").find(".jcrop-holder").length > 0) {
        $(".jcrop-holder").remove();
    }

    if ($("#video_embed_txt").val().indexOf('youtube') > -1) {
        $("#" + media_id + " .media_img").attr("src", $("#video_viewport img").attr("src"));
        $("#" + media_id + " .media_img").css("display", "block");
        $("#" + media_id + " .media_dropbox").css("display", "none");
        $(".close").click();
        if ($("#" + media_id).find("img").length > 0) {
            //alert($("#" + media_id + " .media_img").attr("src"));
            if ($("#" + media_id + " .media_img").attr("src") != "") {
                $("#" + media_id).hover(
                    function () {
                        $("#" + media_id + " .controls").fadeIn('fast');
                    },
                    function () {
                        $("#" + media_id + " .controls").fadeOut('fast');
                    });
            }
        }
    } else {
        $("#edit_modal_body").prepend(data);
        $('#edit_modal_body').prepend('<img id="my-cropped-image" src="#" style="display:none;" />');
        $("#edit_modal_body img:nth-child(2)").attr("id", "crop_target");
        var path = $("#edit_modal_body img:nth-child(2)").attr("src");
        path = path.split("?");
        path = path[0];
        $("#edit_modal_body img:nth-child(2)").attr("src", path);
        //$('#crop_target').attr("src", 'path/to/image');
        if ($("#my-cropped-image").attr("src") != "") {
            var originalImageHeight = $("#my-cropped-image").naturalHeight;
        } else if ($("#crop_target").attr("src") != ""){
        }
        $("#crop_target").Jcrop({
            onChange: setCoordsAndImgSize,
            aspectRatio: 2,
            boxWidth: 598
            //trueSize: [originalImageWidth, originalImageHeight]
        });
        var base_url = window.location.origin;
        var path = $("#crop_target").attr("src").split(base_url);
        path = path[1];
        //path = path[1].replace("/", "\\");
        //path = path.replace("/", "\\");
        path = path.split("?");
        path = path[0];
        $("#crop_target").attr("src", path);
        $("#pictureModal_toggle_btn").click();
    }
}

function setCoordsAndImgSize(e) {
    imageCropWidth = e.w;
    imageCropHeight = e.h;
    cropPointX = e.x;
    cropPointY = e.y;
}

function cropImage() {
    if (imageCropWidth == 0 && imageCropHeight == 0) {
        alert("Please select crop area.");
        return;
    }

    if (cropFlag) {
        var path = $("#my-cropped-image").attr("src");

        path = $("#my-cropped-image").attr("src").split("?");
        path = path[0];
        //path = path[0].replace("/", "\\");
        //path = path.replace("/", "\\");
        $("#crop_target").attr("src", path);
        //alert("test1");
    }

    var cropPath;
    if (editFlag) {
        //cropPath = $("#my-cropped-image").attr("src");
        cropPath = $("#my-cropped-image").attr("src").split("?");
        cropPath = cropPath[0];//.replace("/", "\\");
        //cropPath = cropPath.replace("/", "\\");
        //alert("test2");
    } else {
        cropPath = $("#crop_target").attr("src");
        //alert("test3");
    }

    $.ajax({
        url: '/Article/CropImage',
        type: 'POST',
        beforeSend: function(){
            $(".loading-crop").removeClass("hidden");
        },
        data: {
            imagePath: cropPath,
            cropPointX: cropPointX,
            cropPointY: cropPointY,
            imageCropWidth: imageCropWidth,
            imageCropHeight: imageCropHeight,
            originalHeight: originalHeight,
            originalWidth: originalWidth
        },
        success: function (data) {
            $("#my-cropped-image")
                .attr("src", data.photoPath)
                .show();
            $("#crop_target").css("display", "none");
            cropFlag = true;
            //$("#crop_btn").click(function () {
            //    $(".jcrop-holder").remove();
            //    $("#my-cropped-image").Jcrop({
            //        onChange: setCoordsAndImgSize,
            //        aspectRatio: 1,
            //        boxWidth: 598
            //    });

            //});
            $("#my-cropped-image").removeClass("hide");
            $("#my-cropped-image").css("visibility", "visible");
            $(".jcrop-holder").css("display", "none");
            $(".loading-crop").addClass("hidden");
        },
        error: function (data) {
            alert("error" + data);
        }
    });
}


$(document).ready(function () {
    $(".video_embed_btn").click(function () {
        var media_id = $(this).parent().parent().attr("id");
        $("#video_id").attr("value", media_id);
        $("#videoModal_toggle_btn").click();
    });

    $('.btn_grid').click(function () {
        $('.ul_grid').removeClass('hide');
        $('.ul_list').addClass('hide');
    });

    $('.btn_list').click(function () {
        $('.ul_list').removeClass('hide');
        $('.ul_grid').addClass('hide');
    });

    //if ($(window).width() <= 768) {
    //    $('.nav > li').click(function () {
    //        $('.nav > li').each(function () {
    //            $(this).find('.sub-menu').css('max-height', '0px');
    //        });
    //        var height = 0;
    //        if ($(this).find('.sub-menu').css('max-height') == '0px') {
    //            $(this).prevAll().each(function () {
    //                height = height + $(this).height();
    //            });

    //            $(this).find('.sub-menu').css('max-height', '2000px');
    //            $(this).find('.sub-menu').css('margin-top', height + 'px');
    //        } else {
    //            $(this).find('.sub-menu').css('max-height', '0');
    //            $(this).find('.sub-menu').css('margin-top', '0');
    //        }
    //    });

    //}

    if ($(".panel-body .media").length) {
        var box;
        for (var i = 0; i < $(".panel-body .media").length; i++) {
            box = document.getElementById("media_" + i);
            box.addEventListener("dragenter", OnDragEnter, false);
            box.addEventListener("dragover", OnDragOver, false);
            box.addEventListener("drop", OnDrop, false);
        }
    }

    $("#hl-crop-image").on("click", function (e) {
        if ($(".jcrop-holder").css("display") != "none") {
            e.preventDefault();
            cropImage();
        } else {
            var media_id = $("#media_id").val();
            //alert($("#my-cropped-image").attr("src"));
            $("#" + media_id + " .media_img").attr("src", $("#my-cropped-image").attr("src"));
            var src = $("#my-cropped-image").attr("src").split("?");
            var id = media_id.split("_");
            $("#article_picture_path_" + id[1]).val(src[0]);
            $("#" + media_id + " .media_img").css("display", "block");
            $("#" + media_id + " .media_dropbox").css("display", "none");
            $("#image_edit .close").click();

            $("#" + media_id + " .media_img").attr("src", $("#my-cropped-image").attr("src"));

            if ($("#" + media_id).find("img").length > 0) {
                //alert($("#" + media_id + " .media_img").attr("src"));
                if ($("#" + media_id + " .media_img").attr("src") != "") {
                    $("#" + media_id).hover(
                        function () {
                            $("#" + media_id + " .controls").fadeIn('fast');
                        },
                        function () {
                            $("#" + media_id + " .controls").fadeOut('fast');
                        });
                }
            }
            $('#' + media_id + ' .media_img').attr('edit', true);
        }
        cropFlag = false;
    });
    
    $("#crop_cancel").on("click", function (e) {
        var media_id = $(this).parent().children("input").val();
        if ($("#" + media_id + " .media_img").attr("src") == "") {
            $("#" + media_id + " .media_img").removeAttr("hidden");
        }
        $("#" + media_id + " .media_dropbox").css("display", "block");
        $("#" + media_id + " .article_picture_btn").val("");
        if ($(".jcrop-holder").css("display") == "none") {
            $(".jcrop-holder").css("display", "block");
            $("#my-cropped-image").css("display", "none");
            $("#my-cropped-image").attr("src", $("#crop_target").attr("src"));
        } else {
            $("#image_edit .close").click();
        }
    });

    $("#image_edit .close").click(function () {
        var media_id = $("#crop_cancel").parent().children("input").val();
        if ($("#" + media_id + " .media_img").attr("src") == "") {
            $("#" + media_id + " .media_img").removeAttr("hidden");
        } else {
            $("#" + media_id + " .media_dropbox").addClass("hide");
        }
        $("#" + media_id + " .article_picture_btn").val("");
    });

    $("#video_embed .close").click(function () {
        var media_id = $("#media_id_vid").val();
        $("#" + media_id + " .media_img").attr("src", "");
        $("#" + media_id + " .media_img").removeAttr("hidden");
        //$("#" + media_id + " .article_picture_btn").val("");
    });

    $("#embed_cancel").on("click", function (e) {
        var media_id = $("#video_id").attr("value")
        $("#" + media_id + " .media_img").attr("src", "");
        $("#" + media_id + " .media_img").removeAttr("hidden");
        $("#" + media_id + " .article_picture_btn").val("");
        if ($(".jcrop-holder").css("display") == "none") {
            $(".jcrop-holder").css("display", "block");
            $("#my-cropped-image").css("display", "none");
        } else {
            $(".close").click();
        }
    });
    
    $(".picture_upload_btn").click(function () {
        var media_id = $(this).parent().parent().attr("id");
        $("#media_id").val(media_id);
        $("#" + media_id + " .article_picture_btn").click();
    });

    $("#embed_btn").click(function () {
        var media_id = $("#video_id").attr("value");
        if ($("#video_viewport").find("img").length > 0) {
            $("#video_viewport img").remove();
        }
        if ($("#video_embed_txt").val().indexOf('iframe') > -1) {
            var result = $("#video_embed_txt").val().split(" ");
            var count = 0;
            var src;
            var id;
            for (var i = 0, l = result.length; i < l; i++) {
                if (result[i].indexOf('src') > -1) {
                    src = result[i].split("=");
                    src = src[1].split('"');
                        id = src[1].split("embed");
                        id = id[1].split("/");
                        id = id[1];
                        var media = media_id.split("_");
                        $("#video_url_" + media[1]).val(id);
                        //$("#article_picture_path_" + media[1]).val("http://img.youtube.com/vi/" + id + "/0.jpg");
                        $("#video_viewport").append("<img src='http://img.youtube.com/vi/" + id + "/0.jpg'>");
                }
            }
        } else {
            if (validateURL($("#video_embed_txt").val()) != false) {
                id = $("#video_embed_txt").val().split("?");
                id = id[1].split("=");
                id = id[1];
                //alert("#video_url_" + media[1]);
                media_id = media_id.split("_");
                $("#video_url_" + media_id[1]).val(id);
                $("#video_viewport").append("<img src='http://img.youtube.com/vi/" + id + "/0.jpg'>");
            } else {
                alert("The link is not correct Youtube link. Please insert a Youtube link.");
            }
        }
    });

    $("#embed_confirm").click(function () {
        var media_id = $("#video_id").attr("value");
        $("#media_id_vid").val(media_id);
        if ($("#modal_video_viewport").find("img").length < 1) {
            if ($("#embedded_video_link").val().indexOf('iframe') > -1) {
                var result = $("#embedded_video_link").val().split(" ");
                var count = 0;
                var src;
                var id;
                for (var i = 0, l = result.length; i < l; i++) {
                    if (result[i].indexOf('src') > -1) {
                        src = result[i].split("=");
                        src = src[1].split('"');
                        id = src[1].split("embed");
                        id = id[1].split("/");
                        id = id[1];
                        var media = media_id.split("_");
                        $("#video_url_" + media[1]).val(id);
                        //$("#article_picture_path_" + media[1]).val("http://img.youtube.com/vi/" + id + "/0.jpg");
                        $("#modal_video_viewport").append("<img src='http://img.youtube.com/vi/" + id + "/0.jpg'>");
                    }
                }
            } else {
                if (validateURL($("#embedded_video_link").val()) != false) {
                    id = $("#embedded_video_link").val().split("?");
                    id = id[1].split("=");
                    id = id[1];
                    var media = media_id.split("_");
                    $("#video_url_" + media[1]).val(id);
                    //$("#article_picture_path_" + media[1]).val("http://img.youtube.com/vi/" + id + "/0.jpg");
                    $("#modal_video_viewport").append("<img src='http://img.youtube.com/vi/" + id + "/0.jpg'>");
                } else {
                    alert("The link is not correct Youtube link. Please insert a Youtube link.");
                }
            }
        } else {
            //var media_id = $("#video_id").val();
            $("#" + media_id + " .media_img").attr("src", $("#modal_video_viewport img").attr("src"));
            $("#" + media_id + " .media_img").css("display", "block");
            $("#" + media_id + " .media_dropbox").css("display", "none");
            
            if ($("#" + media_id).find("img").length > 0) {
                //alert($("#" + media_id + " .media_img").attr("src"));
                if ($("#" + media_id + " .media_img").attr("src") != "") {
                    $("#" + media_id).hover(
                        function () {
                            $("#" + media_id + " .controls").fadeIn('fast');
                        },
                        function () {
                            $("#" + media_id + " .controls").fadeOut('fast');
                        });
                }
            }
            var vid_image = $("#video_embed #modal_video_viewport").children("img").attr("src");
            $("#video_embed .close").click();
            $("#" + media_id + " .media_img").attr("src", vid_image);
        }
    });

    $(".edit_btn").click(function (e) {
        var media_id = $(this).parent().parent().parent().attr("id");
        var id = media_id.split("_");
        if ($("#video_url_" + id[1]).attr("value") == "") {
            $("#pictureModal_toggle_btn").click();
            $("edit_modal_body").find(".jcrop-holder").remove();
            $("#edit_modal_body").find("img").remove();
            $("#edit_modal_body").find("div").remove();
            $('#edit_modal_body').prepend('<img id="my-cropped-image" src="#" style="" />');
            $('#edit_modal_body').prepend('<img id="crop_target" />');
            $("#my-cropped-image").attr("src", $("#" + media_id + " .media_img").attr("src"));
            $("#my-cropped-image").addClass("hide");
            $("#crop_target").attr("src", $("#" + media_id + " .media_img").attr("src"));

            $("#crop_btn").css("display", "block");
            var image = new Image();
            image.src = $("#crop_target").attr("src");
            var originalImageWidth = image.naturalWidth;
            var originalImageHeight = image.naturalHeight;
            $("#crop_btn").click(function () {
                //alert("test");
                $("#crop_target").Jcrop({
                    onChange: setCoordsAndImgSize,
                    aspectRatio: 2,
                    boxWidth: 598,
                    trueSize: [originalImageWidth, originalImageHeight]
                });
                editFlag = true;
            });
        } else {
            embed_video(e);
        }
    });

    
    function validateURL(url) {
        var p = /^(?:https?:\/\/)?(?:www\.)?youtube\.com\/watch\?(?=.*v=((\w|-){11}))(?:\S+)?$/;
        return (url.match(p)) ? RegExp.$1 : false;
    }

    if ($("#article_content").length) {
        //CKEDITOR.replace('article_content');
        CKEDITOR.replace('article_content',
        {
            startupFocus: true,
            filebrowserUploadUrl: '/Article/UploadImage' // you must write path to filemanager where you have copied it.
        });
    }

    //$('#my-select').multiSelect();
});

function edit_upload(media_id) {
    var id = media_id.split("_");
    if ($("#video_url_" + id[1]).attr("value") == "") {
        $("#pictureModal_toggle_btn").click();
        $("edit_modal_body").find(".jcrop-holder").remove();
        $("#edit_modal_body").find("img").remove();
        $("#edit_modal_body").find("div").remove();
        $('#edit_modal_body').prepend('<img id="my-cropped-image" src="#" style="" />');
        $('#edit_modal_body').prepend('<img id="crop_target" />');
        $("#my-cropped-image").attr("src", $("#" + media_id + " .media_img").attr("src"));
        $("#crop_target").attr("src", $("#" + media_id + " .media_img").attr("src"));

        $("#crop_btn").css("display", "block");
        var image = new Image();
        image.src = $('#' + media_id + ' .media_img').attr("src");
        var originalImageWidth = image.width;
        var originalImageHeight = image.height;
        
        $("#crop_btn").click(function () {
            var image = new Image();
            image.src = $('#' + media_id + ' .media_img').attr("src");
            var originalImageWidth = image.width;
            var originalImageHeight = image.height;
            $("#crop_target").Jcrop({
                onChange: setCoordsAndImgSize,
                aspectRatio: 2,
                boxWidth: 598,
                trueSize: [originalImageWidth, originalImageHeight]
            });

            editFlag = true;
        });
    } else {
        embed_video(e);
    }
}

function readURL(input) {
    var media_id = $(input).parent().parent().attr("id");

    if (input.files && input.files[0]) {
        var reader = new FileReader();

        reader.onload = function (e) {

            var img = new Image();
            img.src = e.target.result;
            $('#' + media_id + ' .media_img').attr('src', e.target.result);
            //$("#crop_target").attr("src", e.target.result);
            originalWidth = img.width;
            originalHeight = img.height;
            $('#' + media_id + ' .media_img').attr('edit', false);
            $('#' + media_id + ' .media_img').attr('hidden', true);
            //$("#" + media_id + " .edit_btn").click();
            edit_upload(media_id);
            //$('#' + media_id + " .media_dropbox").css("display", "none");
            if ($(".media #" + media_id).find("img").length > 0) {
                //alert($("#" + media_id + " .media_img").attr("src"));
                if ($("#" + media_id + " .media_img").attr("src") != "") {
                    $(".media #" + media_id).hover(
                        function () {
                            $('#' + media_id + " .controls").fadeIn('fast');
                        },
                        function () {
                            $('#' + media_id + " .controls").fadeOut('fast');
                        });
                }
            }

            $("#my-cropped-image").addClass("hide");
        };

        reader.readAsDataURL(input.files[0]);
    }
}

//window.opener.CKEDITOR.tools.callFunction( funcNum, fileUrl [, data] );