function tagitFunc() {
    $('.articleTags').tagit("destroy");
    // START OF TAGIT FUNCTIONS//
    var availableTags = [];
    var availableTagsID = [];

    $('.articleTags').tagit({
        tagSource: function (search, showChoices) {
            if (search.term.length > 2) {
                $.ajax({
                    async: false,
                    type: "POST",
                    data: { Term: search.term },
                    url: "/Home/GetTags",
                    success: function (Tags) {
                        availableTags = [];
                        availableTagsID = [];
                        $('.header_div_2 .keywords').children('p').each(function () {
                            availableTags.push();
                        });
                        for (i = 0; i < Tags.length; i++) {
                            availableTags.push(Tags[i].TagName);
                            availableTagsID.push(Tags[i].TagID);
                        }
                        showChoices(availableTags);
                    }
                })
            }
        },
        select: true,
        sortable: true,
        allowNewTags: false,
        triggerKeys: ['enter', 'tab'],
        beforeTagAdded: function (event, ui) {
            if ($.inArray(ui.tagLabel, availableTags) == -1) {
                return false;
            }
        }
    });

    $(".articleTags input").attr("placeholder", "+ أضف كلمة بحثية");
    // END OF TAGIT FUNCTIONS //
}

function chooseImageSearchFunc() {
    // start of clicking search button in tags //
    $("#chooseimage .search-btn").click(function () {
        $("#img_container .container").html("");
        var tags = "";
        $('.imageTags').children('li').each(function () {
            var tagname = $(this).children('span').text();
            tags = tags + tagname + ",";
        });
        tags = tags.substring(0, tags.length - 2);
        $("#tagNames").val(tags);
        page = 0;
        $.ajax({
            type: "Post",
            data: { Page: 0, TagNames: tags },
            url: "/Article/GetImagesAjax",
            beforeSend: function () {
                $(".loading").removeClass("hidden");
            },
            success: function (data) {
                alert("1");
                page++;
                $images = $(data);
                $('#img_container .container').append($images);
                $(".loading").addClass("hidden");
                var visibleImageCount = $("#img_container .image-choice").length;
                if (parseInt(visibleImageCount) >= parseInt(allImagesCount)) {
                    done = true;
                } else {
                    done = false;
                }
                processing = false;
            }
        });
        return false;
    });
    // end of clicking search button in tags //
}

// start of up and down buttons //
function upclick(index) {
    var id = index;
    order1 = id;
    order2 = parseInt(order1) + 1;

    title1 = $("#topX_title_" + order1).val();
    title2 = $("#topX_title_" + order2).val();
    $("#topX_title_" + order2).val(title1);
    $("#topX_title_" + order1).val(title2);

    src1 = $("#media_" + order1 + " .media_img").attr("src");
    src2 = $("#media_" + order2 + " .media_img").attr("src");
    $("#media_" + order2 + " .media_img").attr("src", src1);
    $("#media_" + order1 + " .media_img").attr("src", src2);
    if ($("#media_" + order2 + " .media_img").attr("src") == "") {
        $("#media_" + order2 + " .edit-image-btn").addClass("hidden");
        $("#media_" + order2 + " .delete-image-btn").addClass("hidden");
        if ($("#media_" + order1 + " .media_img").attr("src") != "") {
            $("#media_" + order1 + " .media_dropbox").addClass("hidden");
        } else {
            $("#media_" + order1 + " .media_dropbox").removeClass("hidden");
        }
    } else {
        $("#media_" + order2 + " .edit-image-btn").removeClass("hidden");
        $("#media_" + order2 + " .delete-image-btn").removeClass("hidden");
        if ($("#media_" + order1 + " .media_img").attr("src") != "") {
            $("#media_" + order1 + " .media_dropbox").addClass("hidden");
        } else {
            $("#media_" + order1 + " .media_dropbox").removeClass("hidden");
        }
    }
    if ($("#media_" + order1 + " .media_img").attr("src") == "") {
        $("#media_" + order1 + " .edit-image-btn").addClass("hidden");
        $("#media_" + order1 + " .delete-image-btn").addClass("hidden");
        if ($("#media_" + order2 + " .media_img").attr("src") != "") {
            $("#media_" + order2 + " .media_dropbox").addClass("hidden");
        } else {
            $("#media_" + order2 + " .media_dropbox").removeClass("hidden");
        }
    } else {
        $("#media_" + order1 + " .edit-image-btn").removeClass("hidden");
        $("#media_" + order1 + " .delete-image-btn").removeClass("hidden");
        if ($("#media_" + order2 + " .media_img").attr("src") != "") {
            $("#media_" + order2 + " .media_dropbox").addClass("hidden");
        } else {
            $("#media_" + order2 + " .media_dropbox").removeClass("hidden");
        }
    }

    path1 = $("#article_picture_path_" + order1).attr("value");
    path2 = $("#article_picture_path_" + order2).attr("value");
    $("#article_picture_path_" + order2).attr("value", path1);
    $("#article_picture_path_" + order1).attr("value", path2);

    video1 = $("#video_" + order1).attr("value");
    video2 = $("#video_" + order2).attr("value");
    $("#video_" + order2).attr("value", video1);
    $("#video_" + order1).attr("value", video2);

    video_path1 = $("#video_url_" + order1).attr("value");
    video_path2 = $("#video_url_" + order2).attr("value");
    $("#video_url_" + order2).attr("value", video_path1);
    $("#video_url_" + order1).attr("value", video_path2);

    content1 = CKEDITOR.instances['topX_Content_' + order1].getData();
    content2 = CKEDITOR.instances['topX_Content_' + order2].getData();
    CKEDITOR.instances['topX_Content_' + order1].setData(content2);
    CKEDITOR.instances['topX_Content_' + order2].setData(content1);
    return false;
}

function downclick(index) {
    var id = index;
    order1 = id;
    order2 = parseInt(order1) - 1;

    title1 = $("#topX_title_" + order1).val();
    title2 = $("#topX_title_" + order2).val()
    $("#topX_title_" + order2).val(title1);
    $("#topX_title_" + order1).val(title2);

    src1 = $("#media_" + order1 + " .media_img").attr("src");
    src2 = $("#media_" + order2 + " .media_img").attr("src");
    $("#media_" + order2 + " .media_img").attr("src", src1);
    $("#media_" + order1 + " .media_img").attr("src", src2);
    if ($("#media_" + order2 + " .media_img").attr("src") == "") {
        $("#media_" + order2 + " .edit-image-btn").addClass("hidden");
        $("#media_" + order2 + " .delete-image-btn").addClass("hidden");
        if ($("#media_" + order1 + " .media_img").attr("src") != "") {
            $("#media_" + order1 + " .media_dropbox").addClass("hidden");
        } else {
            $("#media_" + order1 + " .media_dropbox").removeClass("hidden");
        }
    } else {
        $("#media_" + order2 + " .edit-image-btn").removeClass("hidden");
        $("#media_" + order2 + " .delete-image-btn").removeClass("hidden");
        if ($("#media_" + order1 + " .media_img").attr("src") != "") {
            $("#media_" + order1 + " .media_dropbox").addClass("hidden");
        } else {
            $("#media_" + order1 + " .media_dropbox").removeClass("hidden");
        }
    }
    if ($("#media_" + order1 + " .media_img").attr("src") == "") {
        $("#media_" + order1 + " .edit-image-btn").addClass("hidden");
        $("#media_" + order1 + " .delete-image-btn").addClass("hidden");
        if ($("#media_" + order2 + " .media_img").attr("src") != "") {
            $("#media_" + order2 + " .media_dropbox").addClass("hidden");
        } else {
            $("#media_" + order2 + " .media_dropbox").removeClass("hidden");
        }
    } else {
        $("#media_" + order1 + " .edit-image-btn").removeClass("hidden");
        $("#media_" + order1 + " .delete-image-btn").removeClass("hidden");
        if ($("#media_" + order2 + " .media_img").attr("src") != "") {
            $("#media_" + order2 + " .media_dropbox").addClass("hidden");
        } else {
            $("#media_" + order2 + " .media_dropbox").removeClass("hidden");
        }
    }

    path1 = $("#article_picture_path_" + order1).attr("value");
    path2 = $("#article_picture_path_" + order2).attr("value");
    $("#article_picture_path_" + order2).attr("value", path1);
    $("#article_picture_path_" + order1).attr("value", path2);

    video1 = $("#video_" + order1).attr("value");
    video2 = $("#video_" + order2).attr("value");
    $("#video_" + order2).attr("value", video1);
    $("#video_" + order1).attr("value", video2);

    video_path1 = $("#video_url_" + order1).attr("value");
    video_path2 = $("#video_url_" + order2).attr("value");
    $("#video_url_" + order2).attr("value", video_path1);
    $("#video_url_" + order1).attr("value", video_path2);

    content1 = CKEDITOR.instances['topX_Content_' + order1].getData();
    content2 = CKEDITOR.instances['topX_Content_' + order2].getData();
    CKEDITOR.instances['topX_Content_' + order1].setData(content2);
    CKEDITOR.instances['topX_Content_' + order2].setData(content1);
    return false;
}
// end of up and down buttons //

function loadMoreFunc() {
    // start of load more images when scrolling //
    $("#img_container").scroll(function () {
        if ($('#testScroll').visible() && processing == false && done == false) {
            processing = true;
            $.ajax({
                type: 'POST',
                data: { Page: page, TagNames: $("#tagNames").val() },
                url: '/Article/GetImagesAjax',
                beforeSend: function () {
                    $(".loading-inline").removeClass("hidden");
                },
                success: function (data) {
                    alert("2");
                    page++;
                    $images = $(data);
                    $('#img_container .container').append($images);
                    var visibleImageCount = $("#img_container .image-choice").length;
                    if (parseInt(visibleImageCount) >= parseInt(allImagesCount)) {
                        done = true;
                    } else {
                        done = false;
                    }
                    processing = false;
                    $(".loading-inline").addClass("hidden");
                },
                error: function () {
                    $(".loading-inline").addClass("hidden");
                }
            });
        }
    });
    // end of load more images when scrolling //
}

function dropzoneFunc() {
    $(document).ready(function () {
        // start of dropzone function for croppping //
        $('.dropzone').html5imageupload({
            onSave: function (data) {
                $.ajax({
                    type: 'POST',
                    data: { data: data.data, left: data.left, top: data.top, imageWidth: data.imageWidth, imageHeight: data.imageHeight, imageOriginalWidth: data.imageOriginalWidth, imageOriginalHeight: data.imageOriginalHeight },
                    url: '/Article/TestCrop',
                    beforeSend: function () {
                        $(".loading").removeClass("hidden");
                    },
                    success: function (data) {
                        $(".loading").addClass("hidden");
                        $("#uploadimagefilename").val("/Temp/" + data);
                        $("#uploadimage .addImage").removeAttr("disabled");
                    },
                    error: function () {
                        $(".loading").addClass("hidden");
                    }
                });
            }
        });
        // end of dropzone function for cropping //
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

// start of edit image button //
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

function addImageBtn(e) {
    var media_id = $(e).parent().parent().attr("id");
    var id = media_id.split('_')[1];
    $("#topXindex").val(id);
}

function uploadImageClick(e) {
    $('#image_edit').modal('hide');
    var media_id = $("#topXindex").val();
    $("#media_" + media_id + " .media_dropbox").addClass("hidden");
    $("#media_" + media_id + " .media_img").attr("src", $("#uploadimagefilename").val());
    $("#article_picture_path_" + media_id).attr("src", $("#uploadimagefilename").val());
    $("#media_" + media_id + " .edit-image-btn").removeClass("hidden");
    $("#media_" + media_id + " .delete-image-btn").removeClass("hidden");
    var originalimage = $(".dropzone > img").attr("src");
    $("#originalimage_" + media_id).val(originalimage);
}

function chooseImageClick(e) {
    var src = $(".image-choice .chosen").attr("src");
    src = src.split("?")[0];
    var media_id = $("#topXindex").val();
    $("#media_" + media_id + " .media_dropbox").addClass("hidden");
    $("#media_" + media_id + " .media_img").attr("src", src);
    $("#article_picture_path_" + media_id).attr("src", src);
    $('#image_edit').modal('hide');
    $("#media_" + media_id + " .edit-image-btn").removeClass("hidden");
    $("#media_" + media_id + " .delete-image-btn").removeClass("hidden");
}

// start post creation validation methods //
$.validator.addMethod("needsSelection", function (value, element) {
    return $(element).multiselect("getChecked").length > 0;
});

$.validator.addMethod("minChars", function (value, element) {
    return value.length > 300;
});

$.validator.addMethod("minCharsShort", function (value, element) {
    return value.length > 10;
});

$.validator.addMethod("ImageOrVideo", function (value, element) {
    img_src = $("#media_0 .media_img").attr("src");
    vid_src = $("#video_url_0").attr("value");
    return ((img_src != "") || (vid_src != ""));
});

$.validator.addMethod("RequiredTag", function (value, element) {
    tags_count = $(".articleTags").children("li").length;
    return (tags_count > 1);
});

$.validator.addMethod("reCaptchaValidate", function (value, element) {
    var googleResponse = jQuery('.g-recaptcha-response').val();
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