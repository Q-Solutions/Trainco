var BASEURL = 'http://trainco-phase1.axial-client.com/api';

//var BASEURL = 'http://test.tpctrainco.com/api';

$(document).ready(function () {
    SubmitRegForm();
});


function SubmitRegForm() {
    // checkout
    $('#reg-submit').on('click', function (e) {
        e.preventDefault();
        $('.form-control.error').removeClass('error');
        $('.error-text').remove();
        var bError = false;
        $('.form-item-wrapper .required').each(function () {
            if ($(this).val() == '') {
                bError = true;
                $(this).addClass('error');
                $(this).after('<span class="error-text">' + $(this).attr('placeholder') + ' is required</span>')
            }
        });
        if (bError)
            return false;
        var formData = CreateFormPostString();
        CheckoutPost(formData);
    });
}

function CreateFormPostString() {
    var $cart = $('.form-container');
    var cartGuid = $cart.data('cart');

    var attendeeList = [];
    $('.form-item-wrapper').each(function () {
        var seminarId = $(this).data('seminar');
        $(this).find('.reg-form').each(function () {
            var attendeeNum = $(this).find('input[name="attendee"]').val();
            var attendeeInc = $(this).find('input[name="attendeeInc"]').val();
            var firstName = $(this).find('input[name="firstname"]').val();
            var lastName = $(this).find('input[name="lastname"]').val();
            var title = $(this).find('input[name="title"]').val();
            var email = $(this).find('input[name="email"]').val();

            var attendeeItem = {
                seminarId: seminarId,
                attendeeNum: attendeeNum,
                attendeeInc: attendeeInc,
                firstName: firstName,
                lastName: lastName,
                title: title,
                email: email
            };

            attendeeList.push(attendeeItem);
        })
    });

    var postData = {
        cartGuid: cartGuid,
        checkoutItems: attendeeList
    }

    return postData;
};

function CheckoutPost(checkoutData) {
    var $checkoutErrMsg = $('.checkout-err-msg'),
  		$loader = $('.checkout-loader'),
  		$regSubmit = $('#reg-submit');

    $regSubmit.css('opacity', 0);
    $loader.show();

    $('input').next('span').remove().css('border-color', '#d7d7d7');

    $.ajax({
        url: BASEURL + '/checkout/submit',
        data: JSON.stringify(checkoutData),
        type: "POST",
        contentType: "application/json"
    }).done(function (successObj) {

        var success = successObj.success;
        var message = successObj.message;

        if (success) {
            window.location.href = '/register/info/';
        }
        else {
            // There was a problem with the form.
            $checkoutErrMsg.html(message).show();
            $regSubmit.css('opacity', 1);
            $loader.hide();

            if (successObj.invalidItems.length > 0) {
                var formElArray = successObj.invalidItems;

                for (var i = 0, l = formElArray.length; i < l; i++) {
                    var formEl = formElArray[i];

                    $('#' + formEl.elementId).after('<span class="error-text">' + formEl.message + '</span>');
                    $('#' + formEl.elementId).css('border-color', 'red');
                }
            }

        }
    }).fail(function (error) {
        $regSubmit.css('opacity', 1).prepend('<p class="checkout-err-msg">An error occurred. Please try again later.</p>');
        $loader.hide();
    });
};