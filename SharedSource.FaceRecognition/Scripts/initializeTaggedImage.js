$(document).ready(function () {

    function onTagClick() {
        alert('Clicked');
    }

    $("img[data-facetags]").each(function () {
        var newParent = $("<div style='position:relative' />");
        $(this).wrap(newParent);

        var faceData = JSON.parse(decodeURI($(this).attr("data-facetags")));

        for (var i = 0; i < faceData.length; i++) {
            var rectangle = faceData[i].FacePosition.split(',');

            $(this).parent().append(
                $("<div class='face-tag' style='position:absolute;'/>")
                .css('left', rectangle[0]+'px')
                .css('top', rectangle[1] + 'px')
                .css('width', rectangle[2] + 'px')
                .css('height', rectangle[3] + 'px')
                .css('border', 'solid 2px red')
                .on("click", onTagClick));
        }
    });
});