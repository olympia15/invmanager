function search(formData){
    // AJAX request using jquery
    $.ajax({
        url: 'Product/SearchAndFilter',
        method: 'GET',
        data: formData,
        success: function(data){ // if get request is successful
            $('#resultsContainer').html(data); // inject search results into the container
        },
        error: function(xhr, status, error) { // if unsuccessfull
            console.error("Error fetching search results: ", error);
        },
        complete: function(data){
            $('#loader').hide(); // hide the loading icon
        }
    });
}

// event listener
$(document).ready(function(){
    $('#searchForm').submit(function(evt){
        evt.preventDefault(); // prevent the form from submitting normally
        $('#loader').show(); // display the loading icon

        // get form data
        var formData = {
            query: $('#query').val(),
            category: $('#category').val()
        }
        search(formData); // call the search() function
    });
});





