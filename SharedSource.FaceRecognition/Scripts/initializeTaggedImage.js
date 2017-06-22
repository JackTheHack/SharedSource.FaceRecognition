$(document).ready(function () {

    function onTagClick() {
        $(".tag-item-selector input[name=selectedTag]").val($(this).attr('data-tagid'));
        $(".tag-item-selector").toggleClass('opened');
    }

    function onTagHover() {
        console.log("Face hover");
    }

    function initializeFaceTaggedImage(element, enableOnClick, wrapRequired) {

        if (wrapRequired) {
            var newParent = $("<div style='position:relative' />");
            element.wrap(newParent);
        } else {
            element.parent().css("position", "relative");
        }

        var faceData = JSON.parse(decodeURI(element.attr("data-facetags")));

        var imageHeight = element.outerHeight();
        var imageWidth = element.outerWidth();

        for (var i = 0; i < faceData.length; i++) {

            var item = faceData[i];

            var rectangle = item.FacePosition.split(',');

            var hasSuggestions = item.Suggestions && item.Suggestions.length;

            //var suggestedPerson = hasSuggestions ? item.Suggestions[0] : null;

            var newElement = $("<div class='face-tag'/>")
                .toggleClass('identified', hasSuggestions)
                .attr('data-tagid', faceData[i].UniqueId)
                .css('left', rectangle[0] / imageWidth + 'px')
                .css('top', rectangle[1] / imageHeight + 'px')
                .css('width', rectangle[2] / imageWidth + 'px')
                .css('height', rectangle[3] / imageHeight + 'px')
                .css('border', 'solid 2px red')
                .on("hover", onTagHover);

            if (enableOnClick) {
                newElement.on("click", onTagClick);
            }

            element.parent().append(newElement);
        }
    }

    function fnImgDisplayed(item) {
        console.log("Image displayed callback");
        console.log(item);
        initializeFaceTaggedImage($(".nGY2ViewerImage"), true, true);
    }

    $(".tag-item-selector .tag-person a").on("click", function () {
        var tagId = $(".tag-item-selector input[name=selectedTag]").val();
        var personId = $(this).attr("data-value");
        $.get('/sitecore/api/faceapi/IdentifyTag/'+ decodeURI(tagId) + '/' + decodeURI(personId), function () {
            $.notiny({ text: 'Tag identifiaction saved!', position: 'left-bottom' });
            $(".face-tag[data-tagid=" + tagId + "]").addClass('identified');
            $(".tag-item-selector").toggleClass('opened');
        });
    });

    $("img[data-facetags]").each(function () {
        initializeFaceTaggedImage($(this), true, true);
    });

    $("#nanogallery2").nanogallery2({
        fnImgDisplayed: fnImgDisplayed
    });
});