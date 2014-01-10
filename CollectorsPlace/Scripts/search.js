
$(document).ready(function () {
    
    $.widget("custom.catcomplete", $.ui.autocomplete, {

         _renderMenu: function (ul, items) {

            var that = this,
            currentCategory = "";

            $.each(items, function (index, item) {

                if (item.category != currentCategory) {
                    ul.append( "<li class='ui-autocomplete-category'>" + item.category + "</li>" );
                    currentCategory = item.category;
                }

                that._renderItemData(ul, item);
            });
         }

    });

    $("#searchbox").catcomplete({
        delay: 0,
        source: function (request, response) {
            $.ajax({
                type: "POST",
                cache: false,
                dataType: "json",
                url: '/Home/Search',
                data: { q: request.term },
                success: function (msg) {
                    response(msg);
                }
            });
        },
        select: function (event, ui) {

            if (ui.item.category == "Collectors") {
                window.location.replace("/User/Index/"+ui.item.id);
            } else if (ui.item.category == "Collections") {
                window.location.replace("/User/Collection/" + ui.item.id);
            } else if (ui.item.category == "Articles") {
                window.location.replace("/User/Article/" + ui.item.id);
            }

            // Input está em this.value
        },
    });



});