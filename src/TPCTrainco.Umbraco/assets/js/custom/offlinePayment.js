$(document).on('click', '.cvv-text', function (e) {
    if ($('span', this).length > 0)
        $('span', this).toggleClass('showing');
});

$(document).on('click', '#button-submit', function (e) {
    var obj = $(this);
    $('.error').removeClass('error');
    $('.field-validation-error').remove();
    var bValid = true;
    try {
        var form = obj.parents('form');
        $('.required', form).each(function () {
            if ($(this).val() == '') {
                $(this).addClass('error');
                $(this).after('<span class="field-validation-error">' + $(this).attr('placeholder') + ' is required</span>');
                bValid = false;
            }
        });
        var amount = $('input.amount', form)
        if (!amount.hasClass('error') && parseInt(amount.val()) <= 0) {
            amount.addClass('error');
            amount.after('<span class="field-validation-error">' + amount.attr('placeholder') + ' should be greater than 0</span>');
            bValid = false;
        }
        if(bValid)
            obj.fadeOut(300, function () {
                $('.card-loader').fadeIn();
                $('.processing-msg').fadeIn().text('Processing payment may take several seconds. Please wait...');
            });
    }
    catch (ex) {
        bValid = false;
    }
    return bValid;
});