function orderConfirmation(formData){
    $.ajax({
        url: '/Order/ConfirmOrder',
        method: 'GET',
        data: formData,
        success: function (response) { // if get request is successful
            $('#orderSummaryContainer').html(response); // dynamically update the order summary table
        },
        error: function (xhr, status, error) { // if unsuccessful
            console.error("Error confirming order: ", error);
        },
        complete: function(data) {
            $('#loader').hide(); // hide loading icon
        }
    });
}

$(document).ready(function () {
    $('#confirmOrderForm').submit(function (evt) {
        evt.preventDefault(); // prevent form from submitting normally
        $('#loader').show(); // Show the loader

        // Get the order ID from the form
        var formData = { OrderId: orderId }
        orderConfirmation(formData) // call the orderConfirmation() function
    });
});