    
$(document).ready(function () {
    
    var oTable1 = $('.tabelaDT').dataTable({
        "aaSorting": [[0, 'asc']],
        "bJQueryUI": true,
        "sPaginationType": "full_numbers",
        "sDom": '<"H"Tfr>t<"F"ip>',
        "sScrollY": "250px",
        "iDisplayLength": 10,
        "oLanguage": { "sSearch": "Search" } ,
        "aoColumns": [
                        null,
                        null,
                        { "bSearchable": false, "bSortable": false },
                        { "bSearchable": false, "bSortable": false }
                    ]
    });

    var oTable2 = $('.tabelaDT2').dataTable({
        "aaSorting": [[0, 'asc']],
        "bJQueryUI": true,
        "sPaginationType": "full_numbers",
        "sDom": '<"H"Tfr>t<"F"ip>',
        "sScrollY": "250px",
        "iDisplayLength": 10,
        "oLanguage": { "sSearch": "Search" }
    });

    var oTable3 = $('.tabelaDT3').dataTable({
        "aaSorting": [[0, 'asc']],
        "bJQueryUI": true,
        "sPaginationType": "full_numbers",
        "sDom": '<"H"Tfr>t<"F"ip>',
        "aoColumns": [
                { "bSearchable": false, "bSortable": false },
                null,
                null,
                null
        ],
        "sScrollY": "250px",
        "iDisplayLength": 10,
        "oLanguage": { "sSearch": "Search" }
    });

    var oTable4 = $('.tabelaDT4').dataTable({
        "aaSorting": [[0, 'asc']],
        "bJQueryUI": true,
        "sPaginationType": "full_numbers",
        "sDom": '<"H"Tfr>t<"F"ip>',
        "aoColumns": [
                null,
                null,
                null,
                null,
                { "bSearchable": false, "bSortable": false }
        ],
        "sScrollY": "250px",
        "iDisplayLength": 10,
        "oLanguage": { "sSearch": "Search" }
    });

    /* FIX PARA O RESIZE ACOMPANHAR OS 100%, se corrigir no css basta retirar */

    var update_size = function () {
        $(oTable1).css({ width: $(oTable1).parent().width() });
        oTable1.fnAdjustColumnSizing();

        $(oTable2).css({ width: $(oTable2).parent().width() });
        oTable2.fnAdjustColumnSizing();

        $(oTable3).css({ width: $(oTable3).parent().width() });
        oTable3.fnAdjustColumnSizing();

        $(oTable4).css({ width: $(oTable4).parent().width() });
        oTable4.fnAdjustColumnSizing();

    }

    $(window).resize(function () {
        clearTimeout(window.refresh_size);
        window.refresh_size = setTimeout(function () { update_size(); }, 250);
    });

    /* ------------------------------------------------------------------------ */

});

