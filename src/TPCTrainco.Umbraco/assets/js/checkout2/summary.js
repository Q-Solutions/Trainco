$(document).ready(function () {
    CreateAccount();
    InterestedTrainingTopics();
    SubmitOrder();
    CheckSpeedCheckout();
});

function CreateAccount() {
    var $speedCheckout = $('#speedCheckout');
    var $attendRadio = $('input:radio[name=attend]');
    var $speedCheckoutForm = $('.speedCheckout-form');

    $speedCheckout.on('change', function () {
        CheckSpeedCheckout();
    });

    $attendRadio.on('click', function () {
        CheckPreviousPurchases();
    });

    if ($('.speedCheckout-form').find('.input-validation-error').length > 0) {
        $speedCheckout.prop("checked", true)

        CheckSpeedCheckout();
    }
}

function CheckSpeedCheckout() {
    var $speedCheckout = $('#speedCheckout');
    var $speedCheckoutForm = $('.speedCheckout-form');
    var $CreateAccount = $('#CreateAccount');
    var $SaveAccount = $('#Company_SaveAccount');

    if ($speedCheckout.is(':checked')) {
        $speedCheckoutForm.slideDown('fast');
        $CreateAccount.val('true');
        $SaveAccount.val('true');

    } else {
        $speedCheckoutForm.slideUp('fast');
        $CreateAccount.val('false');
        $SaveAccount.val('false');
    }
}

function CheckPreviousPurchases() {
    var $HasMakePreviousPurchase = $('#Company_HasMakePreviousPurchase');
    var $attendRadio = $('input:radio[name=attend]');

    if ($attendRadio.is(':checked')) {
        $HasMakePreviousPurchase.val('yes');

    } else {
        $HasMakePreviousPurchase.val('no');
    }
}


function InterestedTrainingTopics() {
    $('.interested-topics').click(function () {
        var valueList = $('.interested-topics:checked').map(function () { return this.value; }).get().join(',');

        $('#Company_TrainingTopics').val(valueList);
    });
}

function SubmitOrder() {
    var $submit_button = $('.submit-button');

    $submit_button.on('click', function () {
        $submit_button.fadeOut(300, function () {
            $('.card-loader').fadeIn();
            $('.processing-msg').fadeIn().text('Order processing may take several seconds. Please wait...');
        });

        //setTimeout(function () {
        //    $('.card-loader').fadeOut(100, function () {
        //        $('.processing-msg').text('There was an error processing your order. Please try again.');
        //        $submit_button.fadeIn();
        //    });
        //}, 30000);
    });
}