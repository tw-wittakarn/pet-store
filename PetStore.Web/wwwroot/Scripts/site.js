// site.js - global scripts (jQuery)
var PetStore = PetStore || {};

PetStore.showMessage = function (msg, ok) {
    var cls = ok ? 'alert-success' : 'alert-danger';
    $('#ajax-message').html('<div class="alert ' + cls + '">' + msg + '</div>');
    setTimeout(function () { $('#ajax-message').html(''); }, 3000);
};

$(document).ready(function () {

    // ajax add to cart so the page doesn't reload
    $(document).on('submit', '.add-to-cart-form', function (e) {
        e.preventDefault();
        var $form = $(this);
        $.ajax({
            url: '/Cart/Add',
            type: 'POST',
            data: $form.serialize(),
            success: function (res) {
                PetStore.showMessage(res.message, res.ok);
                if (res.ok) {
                    $('#cart-count').text(res.count);
                    $form.find('button').text('In cart').attr('disabled', 'disabled');
                }
            },
            error: function () {
                alert('Error adding to cart');
            }
        });
        return false;
    });

    // keep the cart badge fresh
    setInterval(function () {
        $.get('/Cart/Count', function (c) { $('#cart-count').text(c); });
    }, 30000);
});
