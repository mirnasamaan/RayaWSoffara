//function showRecaptcha() {
//    Recaptcha.create("6LdhiRQTAAAAAGKaboWCwIsqgpDFEQH4ov3WCYBI", '.g-recaptcha', {
//        theme: 'red',
//        callback: Recaptcha.focus_response_field
//    });
//}

function LoadScript() {
    function getUrlVars() {
        var vars = [], hash;
        var hashes = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
        for (var i = 0; i < hashes.length; i++) {
            hash = hashes[i].split('=');
            vars.push(hash[0]);
            vars[hash[0]] = hash[1];
        }
        return vars;
    }
    var post_type = getUrlVars()["Type"];

    $("#tabs li").removeClass("active");
    $("#my-tab-content div").removeClass("active");
    $("#tabs li:first-child").addClass("active");
    $("#my-tab-content div:first-child").addClass("active");

    ///////////////////////////////////////////////////////////// ARTICLE /////////////////////////////////////////////////////////////

    if ($("div#article").hasClass("active")) {
        // start of submitting the form //
        $("#article .btn_add_article").click(function () {
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
                $("#article_form #submit-btn").append("<input name='SelectedTags[" + index + "]' value='" + tagname + "' type='hidden' />");
            });

        });
        // end of submitting the form //

        $("#article_form").validate({
            ignore: [],
            rules: {
                "newArticle.Title": "required",
                "newArticle.Content": { required: true, minChars: true },
                article_picture_path: { ImageOrVideo: true },
                mySelect: { RequiredTag: true },
                recaptcha: { reCaptchaValidate: true }
            },
            messages: {
                "newArticle.Title": "يجب إدخال عنوان المقالة.",
                "newArticle.Content": {
                    required: "يجب إدخال مضمون المقالة.",
                    minChars: "يجب إدخال على الأقل 300 حرف."
                },
                article_picture_path: "يجب إدخال صورة للمقالة.",
                mySelect: "يجب إدخال كلمة بحثية.",
                recaptcha: { reCaptchaValidate: "برجاء تأكيد هويتك." }
            },
            errorPlacement: function (error, element) {
                if (element.attr("name") == "article_picture_path") {
                    error.insertAfter(".media");
                } else if (element.attr("name") == "newArticle.Content") {
                    error.insertAfter("#cke_Content");
                } else if (element.attr("name") == "mySelect") {
                    error.insertAfter(".articleTags");
                } else if (element.attr("name") == "recaptcha") {
                    error.insertAfter(".g-recaptcha > div > div");
                } else {
                    error.insertAfter(element);
                }
            }
        });

        //CKEDITOR.replace('.ckeditor');
        tagitFunc();
        chooseImageSearchFunc();
        loadMoreFunc();
        dropzoneFunc();

        CKEDITOR.replace('Content');

        ///////////////////////////////////////////////////////////// LISTS /////////////////////////////////////////////////////////////

    } else if ($("div#lists").hasClass("active")) {
        // start of submitting the form //
        $("#lists .btn_add_article").click(function () {
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
                $("#lists_form #submit-btn").append("<input name='SelectedTags[" + index + "]' value='" + tagname + "' type='hidden' />");
            });

            var items = $(".topX_item").length;
            for (var i = items; i > 0; i--) {
                src = $("#media_" + i + " .media_img").attr("src");
                $("#media_" + i + " .TopXImage_" + i).attr("value", src);
                $('#topX_Content_' + i).each(function () {
                        for (instance in CKEDITOR.instances)
                            CKEDITOR.instances[instance].updateElement();
                    });
            }

        });
        // end of submitting the form //

        $("#lists_form").validate({
            ignore: [],
            rules: (function () {
                results = {}
                results["newArticle.Title"] = { required: true }
                results["newArticle.Content"] = { minCharsShort: true }
                results["article_picture_path[0]"] = { ImageOrVideo: true }
                results["mySelect"] = { RequiredTag: true }
                results["recaptcha"] = { reCaptchaValidate: true }
                for (var i = 0; i < $(".topX_item").length; i++) {
                    results["newArticle.ArticleTopXes[" + i + "].TopXTitle"] = { required: true }
                    results["newArticle.ArticleTopXes[" + i + "].TopXImage"] = { required: true }
                    results["newArticle.ArticleTopXes[" + i + "].TopXContent"] = { minCharsShort: true }
                }
                return results;
            })(),
            messages: (function () {
                results = {}
                results["newArticle.Title"] = { required: "يجب إدخال عنوان المقالة." }
                results["newArticle.Content"] = { minCharsShort: "يجب إدخال 10 حروف على الأقل." }
                results["article_picture_path[0]"] = { ImageOrVideo: "يجب إدخال صورة للمقالة." }
                results["mySelect"] = { RequiredTag: "يجب إدخال كلمات بحثية." }
                results["recaptcha"] = { reCaptchaValidate: "برجاء تأكيد هويتك." }
                for (var i = 0; i < $(".topX_item").length; i++) {
                    results["newArticle.ArticleTopXes[" + i + "].TopXTitle"] = { required: "يجب إدخال عنوان المقالة." }
                    results["newArticle.ArticleTopXes[" + i + "].TopXImage"] = { required: "يجب إدخال صورة المقالة." }
                    results["newArticle.ArticleTopXes[" + i + "].TopXContent"] = { minCharsShort: "يجب إدخال 10 كلمة على الأقل." }
                }
                return results;
            })(),
            errorPlacement: function (error, element) {
                for (var i = 0; i < $(".topX_item").length; i++) {
                    if (element.attr("name") == "article_picture_path[0]") {
                        error.insertAfter("#media_0");
                    } else if (element.attr("name") == "newArticle.Content") {
                        error.insertAfter("#cke_topX_Content_0");
                    } else if (element.attr("name") == "mySelect") {
                        error.insertAfter(".articleTags");
                    } else if (element.attr("name") == "newArticle.ArticleTopXes[" + i + "].TopXImage") {
                        error.insertAfter("#media_" + (i + 1));
                    } else if (element.attr("name") == "newArticle.ArticleTopXes[" + i + "].TopXContent") {
                        error.insertAfter("#cke_topX_Content_" + (i + 1));
                    } else if (element.attr("name") == "newArticle.ArticleTopXes[" + i + "].TopXTitle") {
                        error.insertAfter("#topX_title_" + (i + 1));
                    } else if (element.attr("name") == "newArticle.Title") {
                        error.insertAfter("#newArticle_Title");
                    } else if (element.attr("name") == "recaptcha") {
                        error.insertAfter(".g-recaptcha > div > div");
                    }
                }
            }
        });
        
        for (var i = 3; i >= 0; i--) {
            if ($("#topX_Content_" + i).length) {
                CKEDITOR.replace('topX_Content_' + i);
                $("#topX_order_" + i).val(i);
            }
            $("#up_button_3").prop('disabled', true);
            $("#down_button_1").prop('disabled', true);
        }

        $("#add_topX_item").click(function (e) {
            e.preventDefault();
            var count = $("#topX_container > div").length;
            $.ajax({
                type: 'POST',
                url: '/Article/GetTopXAddItemPartial',
                data: { Count: count },
                beforeSend: function () {
                    $(".loading").removeClass("hidden");
                },
                success: function (data) {
                    $item = $(data);
                    $("#topX_container").prepend($item);
                    $("#up_button_" + count).prop('disabled', false);
                    $("#up_button_" + (count + 1)).prop('disabled', true);
                    if ($("#topX_Content_" + (count + 1)).length) {
                        CKEDITOR.replace('topX_Content_' + (count + 1));
                    }
                    $(".loading").addClass("hidden");
                },
                error: function () {
                    $(".loading").addClass("hidden");
                }
            });

            $('#lists_form').data('validator', null);
            $("#lists_form").unbind('validate');
            $("#lists_form").validate({
                ignore: [],
                rules: (function () {
                    results = {}
                    results["newArticle.Title"] = { required: true }
                    results["newArticle.Content"] = { minCharsShort: true }
                    results["article_picture_path"] = { ImageOrVideo: true }
                    results["mySelect"] = { RequiredTag: true }
                    results["recaptcha"] = { reCaptchaValidate: true }
                    for (var i = 0; i < $(".topX_item").length + 1; i++) {
                        results["newArticle.ArticleTopXes[" + i + "].TopXTitle"] = { required: true }
                        results["newArticle.ArticleTopXes[" + i + "].TopXImage"] = { required: true }
                        results["newArticle.ArticleTopXes[" + i + "].TopXContent"] = { minCharsShort: true }
                    }
                    return results;
                })(),
                messages: (function () {
                    results = {}
                    results["newArticle.Title"] = { required: "يجب إدخال عنوان المقالة." }
                    results["newArticle.Content"] = { minCharsShort: "يجب إدخال 10 حروف على الأقل." }
                    results["article_picture_path"] = { ImageOrVideo: "يجب إدخال صورة للمقالة." }
                    results["mySelect"] = { RequiredTag: "يجب إدخال كلمات بحثية." }
                    results["recaptcha"] = { reCaptchaValidate: "برجاء تأكيد هويتك." }
                    for (var i = 0; i < $(".topX_item").length + 1; i++) {
                        results["newArticle.ArticleTopXes[" + i + "].TopXTitle"] = { required: "يجب إدخال عنوان المقالة." }
                        results["newArticle.ArticleTopXes[" + i + "].TopXImage"] = { required: "يجب إدخال صورة المقالة." }
                        results["newArticle.ArticleTopXes[" + i + "].TopXContent"] = { minCharsShort: "يجب إدخال 10 كلمة على الأقل." }
                    }
                    return results;
                })(),
                errorPlacement: function (error, element) {
                    for (var i = 0; i < $(".topX_item").length + 1; i++) {
                        if (element.attr("name") == "article_picture_path") {
                            error.insertAfter("#media_0");
                        } else if (element.attr("name") == "newArticle.Content") {
                            error.insertAfter("#cke_topX_Content_0");
                        } else if (element.attr("name") == "mySelect") {
                            error.insertAfter(".articleTags");
                        } else if (element.attr("name") == "newArticle.ArticleTopXes[" + i + "].TopXImage") {
                            error.insertAfter("#media_" + (i + 1));
                        } else if (element.attr("name") == "newArticle.ArticleTopXes[" + i + "].TopXContent") {
                            error.insertAfter("#cke_topX_Content_" + (i + 1));
                        } else if (element.attr("name") == "newArticle.ArticleTopXes[" + i + "].TopXTitle") {
                            error.insertAfter("#topX_title_" + (i + 1));
                        } else if (element.attr("name") == "newArticle.Title") {
                            error.insertAfter("#newArticle_Title");
                        } else if (element.attr("name") == "recaptcha") {
                            error.insertAfter(".g-recaptcha > div > div");
                        }
                    }
                }
            });
        });

        tagitFunc();
        chooseImageSearchFunc();
        loadMoreFunc();
        dropzoneFunc();

        ///////////////////////////////////////////////////////////// OPINION /////////////////////////////////////////////////////////////

    } else if ($("div#opinion").hasClass("active")) {
        // start of submitting the form //
        $("#opinion .btn_add_article").click(function () {
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
                $("#opinion_form #submit-btn").append("<input name='SelectedTags[" + index + "]' value='" + tagname + "' type='hidden' />");
            });
        });
        // end of submitting the form //

        $("#opinion_form").validate({
            ignore: [],
            rules: {
                "newArticle.Content": { required: true },
                mySelect: { RequiredTag: true },
                recaptcha: { reCaptchaValidate: true }
            },
            messages: {
                "newArticle.Content": {
                    required: "يجب إدخال مضمون المقالة."
                },
                mySelect: "يجب إدخال كلمة بحثية.",
                recaptcha: { reCaptchaValidate: "برجاء تأكيد هويتك." }
            },
            errorPlacement: function (error, element) {
                if (element.attr("name") == "mySelect") {
                    error.insertAfter(".articleTags");
                } else if (element.attr("name") == "recaptcha") {
                    error.insertAfter(".g-recaptcha > div > div");
                } else {
                    error.insertAfter(element);
                }
            }
        });

        tagitFunc();
        chooseImageSearchFunc();
        loadMoreFunc();
        dropzoneFunc();



        ///////////////////////////////////////////////////////////// IMAGE /////////////////////////////////////////////////////////////

    } else if ($("div#image").hasClass("active")) {
        // start of submitting the form //
        $("#image .btn_add_article").click(function () {
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
                $("#image_form #submit-btn").append("<input name='SelectedTags[" + index + "]' value='" + tagname + "' type='hidden' />");
            });
        });
        // end of submitting the form //

        $("#image_form").validate({
            ignore: [],
            rules: {
                article_picture_path: { ImageOrVideo: true },
                mySelect: { RequiredTag: true },
                recaptcha: { reCaptchaValidate: true }
            },
            messages: {
                article_picture_path: "يجب إدخال صورة للمقالة.",
                mySelect: "يجب إدخال كلمة بحثية.",
                recaptcha: { reCaptchaValidate: "برجاء تأكيد هويتك." }
            },
            errorPlacement: function (error, element) {
                if (element.attr("name") == "article_picture_path") {
                    error.insertAfter(".media");
                } else if (element.attr("name") == "newArticle.Content") {
                    error.insertAfter("#cke_article_content");
                } else if (element.attr("name") == "mySelect") {
                    error.insertAfter(".articleTags");
                } else if (element.attr("name") == "recaptcha") {
                    error.insertAfter(".g-recaptcha > div > div");
                } else {
                    error.insertAfter(element);
                }
            }
        });
        
        tagitFunc();
        chooseImageSearchFunc();
        loadMoreFunc();
        dropzoneFunc();



        ///////////////////////////////////////////////////////////// VIDEO /////////////////////////////////////////////////////////////

    } else if ($("div#video").hasClass("active")) {
        // start of submitting the form //
        $("#video .btn_add_article").click(function () {
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
                $("#video_form #submit-btn").append("<input name='SelectedTags[" + index + "]' value='" + tagname + "' type='hidden' />");
            });

        });
        // end of submitting the form //

        $("#video_form").validate({
            ignore: [],
            rules: {
                article_picture_path: { ImageOrVideo: true },
                mySelect: { RequiredTag: true },
                recaptcha: { reCaptchaValidate: true }
            },
            messages: {
                article_picture_path: "يجب إدخال فيديو للمقالة.",
                mySelect: "يجب إدخال كلمة بحثية.",
                recaptcha: { reCaptchaValidate: "برجاء تأكيد هويتك." }
            },
            errorPlacement: function (error, element) {
                if (element.attr("name") == "article_picture_path") {
                    error.insertAfter(".media");
                } else if (element.attr("name") == "mySelect") {
                    error.insertAfter(".articleTags");
                } else if (element.attr("name") == "recaptcha") {
                    error.insertAfter(".g-recaptcha > div > div");
                } else {
                    error.insertAfter(element);
                }
            }
        });
        
        tagitFunc();
        chooseImageSearchFunc();
        loadMoreFunc();
        dropzoneFunc();
    }
}
