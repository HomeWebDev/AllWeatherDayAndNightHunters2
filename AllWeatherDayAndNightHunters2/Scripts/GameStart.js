/// <reference path="../Scripts/jquery-2.2.3.js" />
/// <reference path="../Scripts/jquery-ui-1.11.4.min.js"/>

$(function () {
    //var t = $("#radio");
    ////$("#radio").buttonset();
    function runEffect() {
            // get effect type from
            // most effect types need no options passed by default
        var options = {};
            // run the effect
        $( "#Container" ).show( "fold", options, 500, callback );
    }
    //callback function to bring a hidden box back
    function callback() {
        setTimeout(function() {
            //$("#Container:visible").removeAttr("style").show();
            $("#Container:visible").show();
        }, 1000 );
    }
        // set effect from select menu value
    $( "#radio" ).click(function() {
        runEffect();
    });
    $( "#Container" ).hide();
});