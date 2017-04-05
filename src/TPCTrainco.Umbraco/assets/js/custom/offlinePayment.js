$(document).on('click', '.cvv-text', function (e) {
    if ($('span', this).length > 0)
        $('span', this).toggleClass('showing');
});

$(document).on('click', '#button-submit', function (e) {
    var obj = $(this);
    $('.field-validation-error').remove();
    var bValid = true;
    try {
        var form = obj.parents('form');
        $('input.required', form).each(function () {
            if ($(this).val() == '') {
                $(this).after('<span class="field-validation-error">' + $(this).attr('placeholder') + ' is required</span>');
                bValid = false;
            }
        });
        if(bValid)
            obj.fadeOut(300, function () {
                $('.card-loader').fadeIn();
                $('.processing-msg').fadeIn().text('Order processing may take several seconds. Please wait...');
            });
    }
    catch (ex) {
        bValid = false;
    }
    return bValid;
});