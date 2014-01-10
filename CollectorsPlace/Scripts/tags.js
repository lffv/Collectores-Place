
$(document).ready(function(){
    
    /* Nota: Para alterar os tamanhos max-heigth e width, está no css */

    /* Permite editar */
    $("#tags").tagit({
        autocomplete: {delay: 0, minLength: 3},
        allowSpaces: true,
        caseSensitive: false,
        tagLimit: 20,
        tagSource: function (search, showChoices) {
            var that = this;
            $.ajax({
                type: "POST",
                url: "/Collections/gettags",
                data: search,
                success: function (choices) {
                    showChoices(that._subtractArray(choices, that.assignedTags()));
                }
            });
        },
        preprocessTag: function (val) {

            // Nao há tags com menos de 3 caracteres...

            if (!val) return '';

            if (val.length < 3) return '';

            return val;
        }
    });

    /* Apenas de leitura */
    $("#tagsro").tagit({
        readOnly: true
    });

});