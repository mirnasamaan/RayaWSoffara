var username;

// START OF GETTING POST TYPES FUNCTION //
$.ajax({
    async: false,
    type: "GET",
    url: "/Home/GetPostTypes",
    success: function (posts) {
        for (i = 0; i < posts.length; i++) {
            var color;
            var icon;
            switch (parseInt(posts[i].PostTypeID)) {
                case 1: color = "orange";
                    icon = "<i class='fa fa-newspaper-o' title='مقالة'></i>"; break;
                case 2: color = "red";
                    icon = "<i class='fa fa-list-ol' title='مقالة توب X'></i>"; break;
                case 3: color = "green";
                    icon = "<i class='fa fa-exclamation' title='رأي'></i>"; break;
                case 4: color = "blue";
                    icon = "<i class='fa fa-picture-o' title='صورة'></i>"; break;
                case 5: color = "brown";
                    icon = "<i class='fa fa-video-camera' title='فيديو'></i>"; break;
                default: color = "black"; break;
            }
            html = '<div class="pull-left checkbox-item-wrapper ' + color + '">';
            var id = "";
            if (posts[i].PostTypeID == 2) {
                var str = decodeURI(posts[i].PostTypeName);
                id = "توب";
            } else {
                id = decodeURI(posts[i].PostTypeName);
            }
            html += '<div class="' + color + ' pull-left">' + icon + '</div>';
            html += '<input class="' + id + '" type="checkbox" name="PostType" value="' + posts[i].PostTypeID + '">';
            html += '<label>' + decodeURI(posts[i].PostTypeName) + '</label></div>';

            $(".header_div_4 .checkbox-wrapper.hidden-xs > div").append(html);

            html = '<div class="pull-left checkbox-item-wrapper col-xs-6 ' + color + '">';
            var id = "";
            if (posts[i].PostTypeID == 2) {
                var str = decodeURI(posts[i].PostTypeName);
                id = "توب";
            } else {
                id = decodeURI(posts[i].PostTypeName);
            }
            html += '<div class="' + color + ' pull-left">' + icon + '</div>';
            html += '<input class="' + id + '" type="checkbox" name="PostType" value="' + posts[i].PostTypeID + '">';
            html += '<label>' + decodeURI(posts[i].PostTypeName) + '</label></div>';

            $(".header_div_4 .visible-xs").append(html);
        }
    }
});
// END OF GETTING POST TYPES FUNCTION //

// START OF HEADER 2 RIGHT AND LEFT NAVIGATION //
$(".header_div_2 .right").click(function () {
    event.preventDefault();
    $('.header_div_2 .keywords').animate({
        scrollLeft: "-=100px"
    }, "fast");
});
$(".header_div_2 .left").click(function () {
    event.preventDefault();
    $('.header_div_2 .keywords').animate({
        scrollLeft: "+=100px"
    }, "fast");
});
// END OF HEADER 2 RIGHT AND LEFT NAVIGATION //

//start of show/hide view button //
var page = 1;
var displayed_posts_count = parseInt($(".visible-lg .grid-item").length);
var all_posts_count = parseInt($("#AllActivePostsCount").val());
if (displayed_posts_count >= all_posts_count) {
    $("#viewMore").addClass("hidden");
}
//end of show/hide view button //

// START OF DETECTING SCROLL VALUE TO DISPLAY HEADERS //
$(window).scroll(function (event) {
    var scroll_value = $(window).scrollTop();
    if (parseInt(scroll_value) >= 125) {
        //$(".content-wrapper.duplicate").removeClass("hidden");
        $(".content-wrapper.duplicate").css("top", "60px").css("position", "fixed");
        $(".content-wrapper.duplicate").css("z-index", "99");
    } else if (parseInt(scroll_value) < 125) {
        $(".content-wrapper.duplicate").css("top", "-125px").css("position", "fixed");
        $(".content-wrapper.duplicate").css("z-index", "-1");
        //$(".content-wrapper.duplicate").addClass("hidden");
    }
});
// END OF DETECTING SCROLL VALUE TO DISPLAY HEADERS //

function triggerToggleButton(e) {
    $(e).parent().children("button").click();
}

// START OF ADD AND REMOVE TAGS FUNCTIONS //
function toggleTag(e) {
    var id = $(e).attr("data-id");
    var availableTags = [];
    var availableTagsID = [];
    $('.header_div_2 .keywords div').children('p').each(function () {
        availableTags.push($(this).text());
        availableTagsID.push($(this).parent().children("button").attr("data-id"));
    });

    var original_height;

    $('.myTags').tagit({
        tagSource: availableTags,
        select: true,
        sortable: true,
        allowNewTags: false,
        triggerKeys: ['enter', 'tab'],
        allowSpaces: true,
        beforeTagAdded: function (event, ui) {
            original_height = $(".header_div_3").height();
            if ($.inArray(ui.tagLabel, availableTags) == -1) {
                return false;
            }
        },
        afterTagAdded: function (event, ui) {
            var e = $(".header_div_2 .keywords div p[value='" + ui.tagLabel + "']").parent();
            $(".content-wrapper.original .myTags").tagit("createTag", ui.tagLabel);
            $(".content-wrapper.duplicate .myTags").tagit("createTag", ui.tagLabel);
            var button_element = $('button[data-id=' + $(e).children("button").attr("data-id") + ']');
            $(button_element).css("background-color", "#ab2019");
            $(button_element).attr("data-function", "remove");
            $(button_element).children("i").removeClass("fa-plus");
            $(button_element).children("i").addClass("fa-close");
        },
        beforeTagRemoved: function (event, ui) {
            original_height = $(".header_div_3").height();
            var data_id = $(ui.tag).attr("data-id");
            var button_element = $('button[data-id=' + data_id + ']');
            $(button_element).attr("data-function", "add").css("background-color", "#3bc264");
            $(button_element).children("i").removeClass("fa-close");
            $(button_element).children("i").addClass("fa-plus");
        },
        afterTagRemoved: function (event, ui) {
            var data_id = $(ui.tag).attr("data-id");
            $(".myTags").tagit("removeTag", $(".myTags li[data-id=" + data_id + "]"));
            var button_element = $('button[data-id=' + data_id + ']');
            $(button_element).attr("data-function", "add").css("background-color", "#3bc264");
            $(button_element).children("i").removeClass("fa-close");
            $(button_element).children("i").addClass("fa-plus");
        }
    });

    if ($(e).attr("data-function") == "add") {
        $(".myTags").tagit("createTag", e.id);
        $(".content-wrapper.original .myTags li").eq(-2).attr("data-id", id);
        $(".content-wrapper.duplicate .myTags li").eq(-2).attr("data-id", id);
    }
    else if ($(e).attr("data-function") == "remove") {
        $(".myTags").tagit("removeTag", $(".myTags li[data-id=" + $(e).attr("data-id") + "]"));
    }
}
// END OF ADD AND REMOVE TAGS FUNCTIONS //

$(function () {
    // START OF TAG-IT FUNCTIONS //
    var availableTags = [];
    var availableTagsID = [];

    $('.myTags').tagit({
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
        },
        afterTagAdded: function (event, ui) {
            //alert("test");
            var e = $(".header_div_2 .keywords div p[value='" + ui.tagLabel + "']").parent();
            $(".content-wrapper.original .myTags").tagit("createTag", ui.tagLabel);
            $(".content-wrapper.duplicate .myTags").tagit("createTag", ui.tagLabel);
            $(".myTags li").eq(-2).attr("data-id", $(e).children("button").attr("data-id"));
            $(".content-wrapper.original .myTags li").eq(-2).attr("data-id", $(e).children("button").attr("data-id"));
            $(".content-wrapper.duplicate .myTags li").eq(-2).attr("data-id", $(e).children("button").attr("data-id"));
            var button_element = $('button[data-id=' + $(e).children("button").attr("data-id") + ']');
            $(button_element).css("background-color", "#ab2019");
            $(button_element).attr("data-function", "remove");
            $(button_element).children("i").removeClass("fa-plus");
            $(button_element).children("i").addClass("fa-close");
        },
        beforeTagRemoved: function (event, ui) {
            var data_id = $(ui.tag).attr("data-id");
            var button_element = $('button[data-id=' + data_id + ']');
            $(button_element).attr("data-function", "add").css("background-color", "#3bc264");
            $(button_element).children("i").removeClass("fa-close");
            $(button_element).children("i").addClass("fa-plus");
        },
        afterTagRemoved: function (event, ui) {
            var data_id = $(ui.tag).attr("data-id");
            $(".myTags").tagit("removeTag", $(".myTags li[data-id=" + data_id + "]"));
        }
    });

    $(".header_div_3 .tagit-new input").attr("placeholder", "+ أضف كلمة بحثية");
    // END OF TAG-IT FUNCTIONS //

    //START OF GETTING QUERYSTRING VALUES
    var vars = [], hash;
    var hashes = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
    for (var i = 0; i < hashes.length; i++) {
        hash = hashes[i].split('=');
        vars.push(hash[0]);
        vars[hash[0]] = hash[1];
    }
    var displayed_posts_count = parseInt($(".grid-item").length);
    var all_posts_count = parseInt($("#AllActivePostsCount").val());
    if (displayed_posts_count >= all_posts_count) {
        $("#viewMore").addClass("hidden");
    }

    username = (vars["Username"] == null) ? "" : vars["Username"];
    var posts_qs = (vars["posts"] == null) ? "" : vars["posts"];
    var tags_qs = (vars["tags"] == null) ? "" : vars["tags"];
    if (vars["posts"] != null && vars["posts"] != "") {
        var posts = vars["posts"].split(',');
        for (var i = 0; i < posts.length; i++) {
            $('.' + decodeURI(posts[i])).prop("checked", true);
            if (decodeURI(posts[i]) == "الكل") {
                $(".checkbox-item-wrapper input").prop("checked", true);
            }
        }
    }

    if (vars["tags"] != null) {
        var tags = vars["tags"].split(',');

        var avalableTags = [];
        for (var i = 0; i < tags.length; i++) {
            availableTags[i] = decodeURI(tags[i]);
        }
        $('.myTags').tagit({
            tagSource: availableTags,
            select: true,
            sortable: true,
            allowNewTags: false,
            triggerKeys: ['enter', 'tab'],
            allowSpaces: true,
            beforeTagAdded: function (event, ui) {
                if ($.inArray(ui.tagLabel, availableTags) == -1) {
                    return false;
                }
            },
            afterTagAdded: function (event, ui) {
                var e = $(".header_div_2 .keywords div p[value='" + ui.tagLabel + "']").parent();
                $.ajax({
                    type: "POST",
                    url: "/Article/GetTagIdbyName",
                    data: { tagName: ui.tagLabel },
                    success: function (tagId) {
                        $(".myTags li").eq(-2).attr("data-id", tagId);
                    }
                });
                //$(".myTags li").eq(-2).attr("data-id", $(e).children("button").attr("data-id"));
                $(".content-wrapper.original .myTags").tagit("createTag", ui.tagLabel);
                $(".content-wrapper.duplicate .myTags").tagit("createTag", ui.tagLabel);
                var button_element = $('button[data-id=' + $(e).children("button").attr("data-id") + ']');
                $(button_element).css("background-color", "#ab2019");
                $(button_element).attr("data-function", "remove");
                $(button_element).children("i").removeClass("fa-plus");
                $(button_element).children("i").addClass("fa-close");
            },
            beforeTagRemoved: function (event, ui) {
                var data_id = $(ui.tag).attr("data-id");
                //alert(data_id);
                var element = $('button[data-id=' + data_id + ']');
                $(element).attr("data-function", "add").css("background-color", "#3bc264");
                $(element).children("i").removeClass("fa-close");
                $(element).children("i").addClass("fa-plus");
            },
            afterTagRemoved: function (event, ui) {
                var data_id = $(ui.tag).attr("data-id");
                $(".myTags").tagit("removeTag", $(".myTags li[data-id=" + data_id + "]"));
            }
        });
        for (var i = 0; i < tags.length; i++) {
            $(".myTags").tagit("createTag", decodeURI(tags[i]));
            var e = $(".header_div_2 .keywords div p[value='" + decodeURI(tags[i]) + "']").parent();
            $(".myTags li").eq(-2).attr("data-id", $(e).children("button").attr("data-id"));
            $(e).children("button").css("background-color", "#ab2019");
            $(e).children("button").attr("data-function", "remove");
            $(e).children("button").children("i").removeClass("fa-plus");
            $(e).children("button").children("i").addClass("fa-close");
        }
    }
    //END OF GETTING QUERYSTRING VALUES

    // START OF CHECKBOX CHECK //
    $(".checkbox-item-wrapper label").click(function () {
        var id = $(this).parent().children("input").attr("class");
        if ($(this).parent().children("input").prop("checked")) {
            $("." + id).prop("checked", false);
            $(".الكل").prop("checked", false);
            if ($(this).parent().children("input").attr("class") == "الكل") {
                window.location.href = "/?posts=&tags=&Page=0&Username=" + username;
                return;
            }
        } else {
            $("." + id).prop("checked", true);
        }

        var posts_qs = "";
        $('.checkbox-wrapper.hidden-xs > div').children().each(function () {
            if ($(this).children('input').is(":checked")) {
                posts_qs = posts_qs + $(this).children('input').attr("class") + ",";
            }
        });
        posts_qs = posts_qs.substring(0, posts_qs.length - 1);

        var controller = window.location.href.split('?')[0];
        controller = controller.split('/')[controller.split('/').length - 1];
        if (controller == "UserPosts") {
            window.location.href = "/UserPosts?posts=" + posts_qs + "&tags=" + tags_qs + "&Page=0&Username=" + username;
        } else {
            window.location.href = "/?posts=" + posts_qs + "&tags=" + tags_qs + "&Page=0&Username=" + username;
        }
    });
    // END OF CHECKBOX CHECK //

    // START OF GETTING FEATURED TAGS //
    $.ajax({
        async: false,
        type: "GET",
        url: "/Home/GetFeaturedTags",
        success: function (Tags) {
            for (i = 0; i < Tags.length; i++) {
                html = '<div><p class="pull-left" onclick="triggerToggleButton(this)" value="' + Tags[i].TagName + '">' + Tags[i].TagName + '</p>';
                html += '<button class="pull-right" data-function="add" onclick="toggleTag(this)" id="' + Tags[i].TagName + '" data-id="' + Tags[i].TagID + '"><i class="fa fa-plus"></i></button>';
                html += '</div>';

                $(".header_div_2 .keywords").append(html);
            }
        }
    });
    // END OF GETTING FEATURED TAGS //

    // START OF FEATURED TAGS HEADER TOGGLE BUTTON //
    $(".content-wrapper.original .toggle-btn").click(function () {
        $(".header_div_2.original").slideToggle(300);
    });
    $(".content-wrapper.duplicate .toggle-btn").click(function () {
        $(".header_div_2.duplicate").slideToggle(300);
    });
    // END OF FEATURED TAGS HEADER TOGGLE BUTTON //

});

//START OF SEARCH CLICK FUNCTION
$(".search").click(function () {
    var tags_qs = "";
    $('.content-wrapper.original .myTags').children('li').each(function () {
        var tagname = $(this).children('span').text();
        tags_qs = tags_qs + tagname + ",";
    });
    tags_qs = tags_qs.substring(0, tags_qs.length - 2);
    var posts_qs = "";
    $('.checkbox-wrapper.hidden-xs > div').children().each(function () {
        if ($(this).children('input').is(":checked")) {
            posts_qs = posts_qs + $(this).children('input').attr("class") + ",";
        }
    });
    posts_qs = posts_qs.substring(0, posts_qs.length - 1);

    var controller = window.location.href.split('?')[0];
    controller = controller.split('/')[controller.split('/').length - 1];
    if (controller == "") {
        window.location.href = "/?posts=" + posts_qs + "&tags=" + tags_qs + "&Page=0&Username=" + username;
    } else if (controller == "UserPosts") {
        window.location.href = "/UserPosts?posts=" + posts_qs + "&tags=" + tags_qs + "&Page=0&Username=" + username;
    }
});
//END OF SEARCH CLICK FUNCTION 

//start of view more button click function //
function viewMore(e) {
    var vars = [], hash;
    var hashes = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
    for (var i = 0; i < hashes.length; i++) {
        hash = hashes[i].split('=');
        vars.push(hash[0]);
        vars[hash[0]] = hash[1];
    }

    username = (vars["Username"] == null) ? "" : vars["Username"];
    var posts_qs = (vars["posts"] == null) ? "" : vars["posts"];
    var tags_qs = (vars["tags"] == null) ? "" : vars["tags"];

    var controller = window.location.href.split('?')[0];
    controller = controller.split('/')[controller.split('/').length - 1];

    if (controller == "Popular") {
        $.ajax({
            type: "POST",
            data: { count: page },
            url: "/Home/Popular",
            beforeSend: function () {
                $(".loading").removeClass("hidden");
            },
            success: function (posts) {
                page++;
                $posts = $(posts);
                $(".loading").addClass("hidden");
                $('.grid').append($posts).masonry('appended', $posts);
                var displayed_posts_count = parseInt($(".grid-item").length);
                var all_posts_count = parseInt($("#AllActivePostsCount").val());
                if (displayed_posts_count >= all_posts_count) {
                    $("#viewMore").addClass("hidden");
                }
            }
        });
    } else if (controller == "UserPosts") {
        $.ajax({
            type: "POST",
            data: { posts: posts_qs, tags: tags_qs, count: page },
            url: "/Account/UserPosts",
            beforeSend: function () {
                $(".loading").removeClass("hidden");
            },
            success: function (posts) {
                page++;
                $posts = $(posts);
                $(".loading").addClass("hidden");
                $('.grid').append($posts).masonry('appended', $posts);
                var displayed_posts_count = parseInt($(".grid-item").length);
                var all_posts_count = parseInt($("#AllActivePostsCount").val());
                if (displayed_posts_count >= all_posts_count) {
                    $("#viewMore").addClass("hidden");
                }
            }
        });
    } else {
        $.ajax({
            type: "POST",
            data: { posts: posts_qs, tags: tags_qs, count: page },
            url: "/Home/Index",
            beforeSend: function () {
                $(".loading").removeClass("hidden");
            },
            success: function (posts) {
                page++;
                $posts = $(posts);
                $(".loading").addClass("hidden");
                $('.grid').append($posts).masonry('appended', $posts);
                var displayed_posts_count = parseInt($(".grid-item").length);
                var all_posts_count = parseInt($("#AllActivePostsCount").val());
                if (displayed_posts_count >= all_posts_count) {
                    $("#viewMore").addClass("hidden");
                }
            }
        });
    }

    return false;
}
//end of view more button click function //