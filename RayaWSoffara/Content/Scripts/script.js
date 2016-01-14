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
    var id = $(this).parent().attr("id");
    var media_id = id.split('_')[1];
    $("#media_" + media_id + " .media_img").attr("src", "");
    $("#article_picture_path_" + media_id).attr("src", "");
    $("#media_" + media_id + " .media_dropbox").removeClass("hidden").css("display", "block");
    $("#media_" + media_id + " .delete-image-btn").addClass("hidden");
    $("#media_" + media_id + " .edit-image-btn").addClass("hidden");
    return false;
});
// end of delete buton //

// start od edit image button //
$(".edit-image-btn").click(function () {
    var media_id = $(this).parent().attr("id");
    var id = media_id.split('_')[1];
    $("#topXindex").val(id);
});
// end of edit image button //

// start of choosing image from images choices modal //
function chooseImage(e) {
    $(".image-choice img").removeClass("chosen");
    $(e).addClass("chosen");
    $("#chooseimage .addImage").removeAttr("disabled");
}
// end of choosing image from images choices modal //

// start of submitting the form //
$("#btn_add_article").click(function () {
    src = $("#media_0 .media_img").attr("src");
    if ($("#video_url_0").attr("value") == "") {
        $("#article_picture_path_0").attr("value", src);
    }
    $('.article_content').each(function () {
        for (instance in CKEDITOR.instances)
            CKEDITOR.instances[instance].updateElement();
    });

    var tags_qs = "";
    $('.articleTags').children('li').each(function (index) {
        var tagname = $(this).children('span').text();
        $("#submit-btn").append("<input name='SelectedTags[" + index + "]' value='" + tagname + "' type='hidden' />");
    });
});
// end of submitting the form //

// start of adding image from image-edit modal //
$(".media .addimage-btn").click(function () {
    var media_id = $(this).parent().parent().attr("id");
    var id = media_id.split('_')[1];
    $("#topXindex").val(id);
});

$("#uploadimage .addImage").click(function () {
    $('#image_edit').modal('hide');
    var media_id = $("#topXindex").val();
    $("#media_" + media_id + " .media_dropbox").addClass("hidden");
    $("#media_" + media_id + " .media_img").attr("src", $("#uploadimagefilename").val());
    $("#article_picture_path_" + media_id).attr("src", $("#uploadimagefilename").val());
    $("#media_" + media_id + " .edit-image-btn").removeClass("hidden");
    $("#media_" + media_id + " .delete-image-btn").removeClass("hidden");
    var originalimage = $(".dropzone > img").attr("src");
    $("#originalimage_" + media_id).val(originalimage);
    return false;
});

$("#chooseimage .addImage").click(function () {
    var src = $(".image-choice .chosen").attr("src");
    src = src.split("?")[0];
    var media_id = $("#topXindex").val();
    $("#media_" + media_id + " .media_dropbox").addClass("hidden");
    $("#media_" + media_id + " .media_img").attr("src", src);
    $("#article_picture_path_" + media_id).attr("src", src);
    $('#image_edit').modal('hide');
    $("#media_" + media_id + " .edit-image-btn").removeClass("hidden");
    $("#media_" + media_id + " .delete-image-btn").removeClass("hidden");
    return false;
});
// end of adding image from image-edit modal //

// start post creation validation methods //
$.validator.addMethod("needsSelection", function (value, element) {
    return $(element).multiselect("getChecked").length > 0;
});

$.validator.addMethod("minChars", function (value, element) {
    return value.length > 300;
});

$.validator.addMethod("ImageOrVideo", function (value, element) {
    img_src = $("#media_0 .media_img").attr("src");
    vid_src = $("#video_url_0").attr("value");
    return ((img_src != "") || (vid_src != ""));
});

$.validator.addMethod("RequiredTag", function (value, element) {
    tags_count = $(".articleTags").children("li").length;
    return (tags_count > 5);
});

$.validator.addMethod("reCaptchaValidate", function (value, element) {
    var googleResponse = jQuery('#g-recaptcha-response').val();
    return googleResponse;
});
// end post creation validation methods //

$(document).ready(function () {
    // start od adding video url //
    $("#embed_confirm").click(function () {
        var media_id = $("#topXindex").val();
        var id = $("#embedded_video_link").val().split("?");
        id = id[1].split("=");
        id = id[1];
        $("#media_" + media_id + " .media_img").attr("src", "http://img.youtube.com/vi/" + id + "/0.jpg");
        $("#media_" + media_id + " .media_img").css("display", "block");
        $("#media_" + media_id + " .media_dropbox").css("display", "none");

        var vid_image = $("#modal_video_viewport").children("img").attr("src");
        $("#media_" + media_id + " .edit-image-btn").removeClass("hidden");
        $("#media_" + media_id + " .delete-image-btn").removeClass("hidden");
        $("#image_edit .close").click();
        $("#media_" + media_id + " .media_img").attr("src", vid_image);
    });
    // end of adding video url //

    //if ($(".article_content").length) {
    //    CKEDITOR.replace('.article_content',
    //    {
    //        startupFocus: true,
    //        filebrowserUploadUrl: '/Article/UploadImage' // you must write path to filemanager where you have copied it.
    //    });
    //}
});