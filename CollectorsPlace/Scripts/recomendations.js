
$(document).ready(function(){

    function actualizaRecomendacoes() {

        $.post("/User/Recomendacoes", {}, function (data) {

            $.each(data, function (key, val) {

                if (val.titulo != null && val.img != null && val.href != null) {

                    var titulo = val.titulo;
                    var limg = val.img;
                    var href = val.href;

                    if (titulo != "" && limg != "" && href != "") {

                        // Adiciona o titulo
                        $("#hmdb").append('<p>' + titulo + '</p>');

                        // Cria o href
                        var ah = $(document.createElement('a'));
                        ah.attr("href", href);

                        // Cria a imagem
                        var img = $(document.createElement('img'));
                        img.attr('src', limg);

                        img.addClass("imagem-perfil");

                        // Adciona a imagem ao href
                        img.appendTo(ah);

                        // Adiciona o href com a imagem à div
                        ah.appendTo("#hmdb");

                    }

                }

            });

        }, "json");

    }

    actualizaRecomendacoes();

    $("#refreshrec").click(function () {

        // Remove as que tem
        $('#hmdb').children().remove();

        // Actualiza
        actualizaRecomendacoes();
    });

});

