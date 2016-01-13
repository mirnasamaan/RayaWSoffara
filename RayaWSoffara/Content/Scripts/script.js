//var imageCropWidth = 0;
//var imageCropHeight = 0;
//var cropPointX = 0;
//var cropPointY = 0;
//var cropFlag = false;
//var editFlag = false;
//var originalWidth = 0;
//var originalHeight = 0;

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

// start of delete image button //
$(".delete-image-btn").click(function () {
    $(".media_img").attr("src", "");
    $("#article_picture_path_0").attr("src", "");
    $(".media_dropbox").removeClass("hidden").css("display", "block");
    $(".delete-image-btn").addClass("hidden");
    $(".edit-image-btn").addClass("hidden");
    return false;
});
// end of delete buton //

// start of choosing image from images choices modal //
function chooseImage(e) {
    $(".image-choice img").removeClass("chosen");
    $(e).addClass("chosen");
    $("#chooseimage .addImage").removeAttr("disabled");
}
// end of choosing image from images choices modal //

$(document).ready(function () {
    $(".video_embed_btn").click(function () {
        var media_id = $(this).parent().parent().attr("id");
        $("#video_id").attr("value", media_id);
        $("#videoModal_toggle_btn").click();
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

    $("#embed_confirm").click(function () {
        var media_id = $("#video_id").attr("value");
        $("#media_id_vid").val(media_id);
        var id = $("#embedded_video_link").val().split("?");
        id = id[1].split("=");
        id = id[1];
        $("#" + media_id + " .media_img").attr("src", "http://img.youtube.com/vi/" + id + "/0.jpg");
            $("#" + media_id + " .media_img").css("display", "block");
            $("#" + media_id + " .media_dropbox").css("display", "none");
            
            if ($("#" + media_id).find("img").length > 0) {
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
            var vid_image = $("#modal_video_viewport").children("img").attr("src");
            $("#image_edit .close").click();
            $("#" + media_id + " .media_img").attr("src", vid_image);
            $(".edit-image-btn").removeClass("hidden");
            $(".delete-image-btn").removeClass("hidden");
        //}
    });
    
    //function validateURL(url) {
    //    var p = /^(?:https?:\/\/)?(?:www\.)?youtube\.com\/watch\?(?=.*v=((\w|-){11}))(?:\S+)?$/;
    //    return (url.match(p)) ? RegExp.$1 : false;
    //}

    if ($("#article_content").length) {
        CKEDITOR.replace('article_content',
        {
            startupFocus: true,
            filebrowserUploadUrl: '/Article/UploadImage' // you must write path to filemanager where you have copied it.
        });
    }
});