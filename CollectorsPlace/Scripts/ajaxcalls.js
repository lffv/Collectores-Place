
$(document).ready(function () {

    $("#frmfollow").submit(function () {

        var p = $(this).attr("action");
        var b = $("#btnfollow");

        $.ajax({
            type: "POST",
            url: p,
            data: $(this).serializeArray(),
        }).done(function (msg) {
            
            if (msg==1) {
                
                var nome = $("#impfl").attr("title"), imgs = $("#impfl").attr("src");

                if (b.val().toLowerCase() == "follow") {    

                    var i = '<img src="'+imgs+'" class="mini-followingImg" title="'+nome+'" />';

                    $('#dvfollowing').append(i);

                    b.val("Unfollow").addClass("unFollow").removeClass("doFollow");

                } else {

                    $('#dvfollowing').children('img[title="'+nome+'"]:first').remove();

                    b.val("Follow").addClass("doFollow").removeClass("unFollow");
                }

            } else {
                b.css("border-color", "red");
            }

        });

        return false;

    });

});

