$(document).ready(function () {

    function selectTagClickHandler() {

        console.log("Tag selected");

        var tagId = $(".tag-item-selector input[name=selectedTag]").val();
        var personId = $(this).attr("data-value");
        $.get('/sitecore/api/faceapi/IdentifyTag/' + decodeURI(tagId) + '/' + decodeURI(personId), function () {
            $.notiny({ text: 'Tag identifiaction saved!', position: 'left-bottom' });
            $(".face-tag[data-tagid=" + tagId + "]").addClass('identified');
            $(".tag-item-selector").toggleClass('opened');
        });
    }

    function onTagClick() {

        console.log("Tag click");

        $(".face-tag").removeClass("selected");        

        var container = $(this).parent();
        var imgElement = container.find("img");
        var tagId = $(this).attr('data-tagid');

        var suggestionsValue = unescape(imgElement.attr('data-suggestions'));
        var suggestions = JSON.parse(suggestionsValue);

        $(".tag-item-selector .suggested-persons-container").toggle(suggestions && suggestions.length > 0 ? true : false);

        var faceSuggestions = suggestions.filter(function (i) { return i.FaceId === tagId });

        $(".tag-item-selector .suggested-persons").empty();
        faceSuggestions.forEach(function(face) {
            $(".tag-item-selector .suggested-persons").append($("<div class='tag-person'><a href='#' data-value='" + face.Data.ID + "'>" + face.Data.Name.replace(/\+/g, ' ') + "</a></div>"));
        });

        $(".tag-item-selector .suggested-persons .tag-person a").on("click", selectTagClickHandler);

        $(".tag-item-selector input[name=selectedTag]").val(tagId);
        $(".tag-item-selector").toggleClass('opened');

        if ($(".tag-item-selector").hasClass("opened")) {
            $(this).addClass('selected');
        }
    }

    function onTagHover() {
        console.log("Face hover");        
    }

    function initializeFaceTaggedImage(element, enableOnClick, wrapRequired) {

        if (wrapRequired) {
            var newParent = $("<div class='face-tag-container'/>");
            element.wrap(newParent);
        } else {
            element.parent().css("position", "relative");
        }

        var faceTagsValue = unescape(element.attr("data-facetags"));

        var identified = unescape(element.attr("data-identified"));

        var suggestionsValue = unescape(element.attr('data-suggestions'));

        if (!faceTagsValue || faceTagsValue.length === 0) return;
            
        var faceData = JSON.parse(faceTagsValue);

        var suggestions = JSON.parse(suggestionsValue);

        var originalSizes = unescape(element.attr("data-imgsize")).split(',');

        var imageHeight = element.outerHeight();
        var imageWidth = element.outerWidth();

        for (var i = 0; i < faceData.length; i++) {

            var item = faceData[i];

            var rectangle = item.FacePosition.split(',');

            var identifiedPersons = identified && identified.length > 0  ? JSON.parse(identified) : null;

            var isIdentified = false;

            var identifiedPerson = identifiedPersons ? identifiedPersons.filter(function(i) { return i.Id === item.UniqueId; }) : null;

            if (identifiedPerson && item) {
                isIdentified = identifiedPerson.length > 0;
            }

            var faceSuggestions = suggestions.filter(function(i) { return i.FaceId === item.UniqueId });

            var hasSuggestions = faceSuggestions && faceSuggestions.length > 0 ? true : false;

            var newElement = $("<div class='face-tag'/>")
                .toggleClass('has-suggestions', hasSuggestions)
                .toggleClass('identified', isIdentified)
                .attr('data-tagid', faceData[i].UniqueId)
                .css('left', parseFloat(rectangle[0].replace('+', '')) / parseFloat(originalSizes[0]) * imageWidth + 'px')
                .css('top', parseFloat(rectangle[1].replace('+', '')) / parseFloat(originalSizes[1]) * imageHeight + 'px')
                .css('width', parseFloat(rectangle[2].replace('+', '')) / parseFloat(originalSizes[0]) * imageWidth + 'px')
                .css('height', parseFloat(rectangle[3].replace('+', '')) / parseFloat(originalSizes[1]) * imageHeight + 'px')
                .css('border-width', '2px')                
                .on("hover", onTagHover);

            if (identifiedPerson && identifiedPerson.length > 0 && identifiedPerson[0].Data) {
                newElement.append($("<div class='tag-info'></div>").text(identifiedPerson[0].Data.Name.replace('+',' ')));
            } else if(hasSuggestions) {
                var firstSuggestion = faceSuggestions[0];
                newElement.append($("<div class='tag-info'></div>").text(firstSuggestion.Data.Name.replace('+', ' ') + ' (' + firstSuggestion.Confidence + ')'));
            }

            if (enableOnClick) {
                newElement.on("click", onTagClick);
            }

            element.parent().append(newElement);
        }
    }
    
    $(".tag-item-selector .tag-person a").on("click", selectTagClickHandler);

    setTimeout(function() {
        $("img[data-facetags]").each(function() {
            initializeFaceTaggedImage($(this), true, true);
        });
    }, 1000);
});