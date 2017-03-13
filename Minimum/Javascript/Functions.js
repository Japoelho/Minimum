/*--------------- [ EFFECTS ] ---------------*/

// [onmouseover], [onmouseout]
// - Fades in/out the text shadow of the [element] on mouse over/out over the [duration] in milliseconds, of the [colorArray].
function textShadowFade(element, event, duration, colorArray) {
    if (element.textShadowFadeAnimation) {
        clearInterval(element.textShadowFadeAnimation);
    }

    if (!element.rgb) {
        if (!element.onmouseover) {
            var nevent = { type: "mouseover" };
            element.onmouseover = function () { textShadowFade(element, nevent, duration, colorArray); }
        }

        if (!element.onmouseout) {
            var nevent = { type: "mouseout" };
            element.onmouseout = function () { textShadowFade(element, nevent, duration, colorArray); }
        }

        element.radius = 3;

        if (colorArray) {
            element.textShadowFadeTargetRGB = colorArray;
        }
        else {
            element.textShadowFadeTargetRGB = [0, 0, 0];
        }

        element.textShadowFadeRGB = [0, 0, 0];
        element.textShadowFadeR = element.textShadowFadeTargetRGB[0] / (duration / 10);
        element.textShadowFadeG = element.textShadowFadeTargetRGB[1] / (duration / 10);
        element.textShadowFadeB = element.textShadowFadeTargetRGB[2] / (duration / 10);
    }

    switch (event.type) {
        case "mouseover":
            {
                element.textShadowFadeAnimation = setInterval(function () {
                    if (element.textShadowFadeRGB[0] < element.textShadowFadeTargetRGB[0]) {
                        element.textShadowFadeRGB[0] += element.textShadowFadeR;
                    }
                    if (element.textShadowFadeRGB[1] < element.textShadowFadeTargetRGB[1]) {
                        element.textShadowFadeRGB[1] += element.textShadowFadeG;
                    }
                    if (element.textShadowFadeRGB[2] < element.textShadowFadeTargetRGB[2]) {
                        element.textShadowFadeRGB[2] += element.textShadowFadeB;
                    }

                    element.style.textShadow = "2px 2px " + element.radius + "px rgb(" + (element.textShadowFadeRGB[0] | 0) + ", " + (element.textShadowFadeRGB[1] | 0) + ", " + (element.textShadowFadeRGB[2] | 0) + ")";

                    if (element.textShadowFadeRGB[0] >= element.textShadowFadeTargetRGB[0] && element.textShadowFadeRGB[1] >= element.textShadowFadeTargetRGB[1] && element.textShadowFadeRGB[2] >= element.textShadowFadeTargetRGB[2]) {
                        clearInterval(element.textShadowFadeAnimation);
                    }
                }, 10);
                break;
            }
        case "mouseout":
            {
                element.textShadowFadeAnimation = setInterval(function () {
                    if (element.textShadowFadeRGB[0] > 0) {
                        element.textShadowFadeRGB[0] -= element.textShadowFadeR;
                    }
                    if (element.textShadowFadeRGB[1] > 0) {
                        element.textShadowFadeRGB[1] -= element.textShadowFadeG;
                    }
                    if (element.textShadowFadeRGB[2] > 0) {
                        element.textShadowFadeRGB[2] -= element.textShadowFadeB;
                    }

                    element.style.textShadow = "2px 2px " + element.radius + "px rgb(" + (element.textShadowFadeRGB[0] | 0) + ", " + (element.textShadowFadeRGB[1] | 0) + ", " + (element.textShadowFadeRGB[2] | 0) + ")";

                    if (element.textShadowFadeRGB[0] <= 0 && element.textShadowFadeRGB[1] <= 0 && element.textShadowFadeRGB[2] <= 0) {
                        clearInterval(element.textShadowFadeAnimation);
                    }
                }, 10);
                break;
            }
    }
}

// - Returns a semi-transparent layer, removes the layer when clicked if [removeOnClick] is set to true, and calls [onRemove] if specified.
function backgroundMask(removeOnClick, onRemove) {
    var background = document.createElement("div");

    background.style.position = "fixed";
    background.style.top = "0px";
    background.style.bottom = "0px";
    background.style.left = "0px";
    background.style.right = "0px";
    background.style.backgroundColor = "black";
    background.style.opacity = "0.5";
    background.style.filter = "alpha(opacity = 50)";

    if (removeOnClick) {
        background.onclick = function () {
            document.body.removeChild(background);
            if (onRemove) {
                onRemove();
            }
        }
    }

    document.body.appendChild(background);
    return background;
}

// - Returns the RGB value of a [color] hexadecimal value.
function colorHexToRgb(color) {
    var r, g, b;
    if (color.charAt(0) == '#') {
        color = color.substr(1);
    }
    r = color.charAt(0) + color.charAt(1);
    g = color.charAt(2) + color.charAt(3);
    b = color.charAt(4) + color.charAt(5);
    r = parseInt(r, 16);
    g = parseInt(g, 16);
    b = parseInt(b, 16);
    return "rgb(" + r + "," + g + "," + b + ")";
}

// - Returns the hexadecimal value of a [color] name.
function colorNameToHex(color) {
    var colors = {
        "aliceblue": "#f0f8ff",
        "antiquewhite": "#faebd7",
        "aqua": "#00ffff",
        "aquamarine": "#7fffd4",
        "azure": "#f0ffff",
        "beige": "#f5f5dc",
        "bisque": "#ffe4c4",
        "black": "#000000",
        "blanchedalmond": "#ffebcd",
        "blue": "#0000ff",
        "blueviolet": "#8a2be2",
        "brown": "#a52a2a",
        "burlywood": "#deb887",
        "cadetblue": "#5f9ea0",
        "chartreuse": "#7fff00",
        "chocolate": "#d2691e",
        "coral": "#ff7f50",
        "cornflowerblue": "#6495ed",
        "cornsilk": "#fff8dc",
        "crimson": "#dc143c",
        "cyan": "#00ffff",
        "darkblue": "#00008b",
        "darkcyan": "#008b8b",
        "darkgoldenrod": "#b8860b",
        "darkgray": "#a9a9a9",
        "darkgreen": "#006400",
        "darkkhaki": "#bdb76b",
        "darkmagenta": "#8b008b",
        "darkolivegreen": "#556b2f",
        "darkorange": "#ff8c00",
        "darkorchid": "#9932cc",
        "darkred": "#8b0000",
        "darksalmon": "#e9967a",
        "darkseagreen": "#8fbc8f",
        "darkslateblue": "#483d8b",
        "darkslategray": "#2f4f4f",
        "darkturquoise": "#00ced1",
        "darkviolet": "#9400d3",
        "deeppink": "#ff1493",
        "deepskyblue": "#00bfff",
        "dimgray": "#696969",
        "dodgerblue": "#1e90ff",
        "firebrick": "#b22222",
        "floralwhite": "#fffaf0",
        "forestgreen": "#228b22",
        "fuchsia": "#ff00ff",
        "gainsboro": "#dcdcdc",
        "ghostwhite": "#f8f8ff",
        "gold": "#ffd700",
        "goldenrod": "#daa520",
        "gray": "#808080",
        "green": "#008000",
        "greenyellow": "#adff2f",
        "honeydew": "#f0fff0",
        "hotpink": "#ff69b4",
        "indianred ": "#cd5c5c",
        "indigo": "#4b0082",
        "ivory": "#fffff0",
        "khaki": "#f0e68c",
        "lavender": "#e6e6fa",
        "lavenderblush": "#fff0f5",
        "lawngreen": "#7cfc00",
        "lemonchiffon": "#fffacd",
        "lightblue": "#add8e6",
        "lightcoral": "#f08080",
        "lightcyan": "#e0ffff",
        "lightgoldenrodyellow": "#fafad2",
        "lightgrey": "#d3d3d3",
        "lightgreen": "#90ee90",
        "lightpink": "#ffb6c1",
        "lightsalmon": "#ffa07a",
        "lightseagreen": "#20b2aa",
        "lightskyblue": "#87cefa",
        "lightslategray": "#778899",
        "lightsteelblue": "#b0c4de",
        "lightyellow": "#ffffe0",
        "lime": "#00ff00",
        "limegreen": "#32cd32",
        "linen": "#faf0e6",
        "magenta": "#ff00ff",
        "maroon": "#800000",
        "mediumaquamarine": "#66cdaa",
        "mediumblue": "#0000cd",
        "mediumorchid": "#ba55d3",
        "mediumpurple": "#9370d8",
        "mediumseagreen": "#3cb371",
        "mediumslateblue": "#7b68ee",
        "mediumspringgreen": "#00fa9a",
        "mediumturquoise": "#48d1cc",
        "mediumvioletred": "#c71585",
        "midnightblue": "#191970",
        "mintcream": "#f5fffa",
        "mistyrose": "#ffe4e1",
        "moccasin": "#ffe4b5",
        "navajowhite": "#ffdead",
        "navy": "#000080",
        "oldlace": "#fdf5e6",
        "olive": "#808000",
        "olivedrab": "#6b8e23",
        "orange": "#ffa500",
        "orangered": "#ff4500",
        "orchid": "#da70d6",
        "palegoldenrod": "#eee8aa",
        "palegreen": "#98fb98",
        "paleturquoise": "#afeeee",
        "palevioletred": "#d87093",
        "papayawhip": "#ffefd5",
        "peachpuff": "#ffdab9",
        "peru": "#cd853f",
        "pink": "#ffc0cb",
        "plum": "#dda0dd",
        "powderblue": "#b0e0e6",
        "purple": "#800080",
        "red": "#ff0000",
        "rosybrown": "#bc8f8f",
        "royalblue": "#4169e1",
        "saddlebrown": "#8b4513",
        "salmon": "#fa8072",
        "sandybrown": "#f4a460",
        "seagreen": "#2e8b57",
        "seashell": "#fff5ee",
        "sienna": "#a0522d",
        "silver": "#c0c0c0",
        "skyblue": "#87ceeb",
        "slateblue": "#6a5acd",
        "slategray": "#708090",
        "snow": "#fffafa",
        "springgreen": "#00ff7f",
        "steelblue": "#4682b4",
        "tan": "#d2b48c",
        "teal": "#008080",
        "thistle": "#d8bfd8",
        "tomato": "#ff6347",
        "turquoise": "#40e0d0",
        "violet": "#ee82ee",
        "wheat": "#f5deb3",
        "white": "#ffffff",
        "whitesmoke": "#f5f5f5",
        "yellow": "#ffff00",
        "yellowgreen": "#9acd32"
    };

    if (typeof colors[color.toLowerCase()] != "undefined") return colors[color.toLowerCase()];

    return false;
}

/*--------------- [ URL FUNCTIONS ] ---------------*/

(function (url) {
    // - Returns the root URL.
    url.root = function () {
        return window.location.origin ? window.location.origin + "/" : window.location.protocol + "//" + window.location.host + "/";
    };

    // - Returns the base URL.
    url.base = function () {
        return new RegExp(/^.*\//).exec(window.location.href);
    };
})(window.url = window.url || {});

/*--------------- [ INPUT EVENT FUNCTIONS ] ---------------*/

(function (input) {
    // - Returns true if the [event] keyCode is numeric or only numeric values.
    input.onlyNumeric = function (event) {
        event = event || window.event;
        switch (event.type) {
            case "keypress":
                {
                    var keyCode = event.which || event.keyCode;
                    if (input.isNotValue(keyCode)) {
                        return true;
                    }

                    return !isNaN(String.fromCharCode(keyCode));
                }
            case "paste":
                {
                    var pasteData = event ? event.clipboardData ? event.clipboardData.getData('text/plain') : "" : window.event.clipboardData ? window.event.clipboardData.getData('text/plain') : "";

                    //if (event.preventDefault) {
                    //    event.stopPropagation();
                    //    event.preventDefault();
                    //}

                    return !isNaN(pasteData);
                }
        }
    };

    // - Returns true if the [event] keyCode matches the regex [pattern].
    input.matchRegex = function (event, pattern) {
        event = event || window.event;
        var keyCode = event.which || event.keyCode;

        if (input.isNotValue(keyCode)) {
            return true;
        }

        var regex = new RegExp(pattern);
        return regExp.test(String.fromCharCode(keyCode));
    };

    // - Formats the [element] value if possible and returns true if the [element] value matches the format of [pattern].
    input.formatValue = function (element, event, pattern) {
        event = event || window.event;

        if (!element.oninput) {
            element.oninput = function () {
                var value = element.value.replace(/[^\w]/g, '');

                for (var i = 0, p = 0; i < value.length; i++, p++) {
                    if (p > pattern.length) {
                        value = value.substr(0, pattern.length);
                        break;
                    }
                    else if (/^[0-9]$/.test(pattern[p])) {
                        if (!/^[0-9]$/.test(value[i])) {
                            value = value.substr(0, i);
                            break;
                        }
                    }
                    else if (/^[a-zA-Z]$/.test(pattern[p])) {
                        if (!/^[a-zA-Z]$/.test(value[i])) {
                            value = value.substr(0, i);
                            break;
                        }
                    }
                    else {
                        value = value.substr(0, i) + pattern[p] + value.substr(i, value.length);
                        i++; p++;
                    }
                }

                element.value = value;
            }
        }

        switch (event.type) {
            case "keypress":
                {
                    var keyCode = event.which || event.keyCode;

                    if (input.isNotValue(keyCode)) {
                        return true;
                    }

                    while (true) {
                        if (element.value.length >= pattern.length || !pattern[element.value.length]) {
                            return false;
                        }
                        else if (/^[0-9]$/.test(pattern[element.value.length])) { //If numeric
                            if (!isNaN(String.fromCharCode(keyCode))) {
                                return true;
                            }
                            else {
                                return false;
                            }
                        }
                        else if (/^[a-zA-Z]$/.test(pattern[element.value.length])) { //If character only
                            if (/^[a-zA-Z]$/.test(String.fromCharCode(keyCode))) {
                                return true;
                            }
                            else {
                                return false;
                            }
                        }
                        else if (String.fromCharCode(keyCode) == pattern[element.value.length]) {
                            return true;
                        }
                        else {
                            element.value += pattern[element.value.length];
                        }
                    }
                    break;
                }
            case "paste":
                {
                    var pasteData = event.clipboardData ? event.clipboardData.getData('text/plain') : "";
                    for (var i = 0; i < pasteData.length; i++) {
                        while (true) {
                            if (element.value.length >= pattern.length || !pattern[element.value.length]) {
                                break;
                            }
                            else if (/^[0-9]$/.test(pattern[element.value.length])) { //If numeric
                                if (!isNaN(pasteData[i])) {
                                    element.value += pasteData[i];
                                    break;
                                }
                                else {
                                    break;
                                }
                            }
                            else if (/^[a-zA-Z]$/.test(pattern[element.value.length])) { //If character only
                                if (/^[a-zA-Z]$/.test(pasteData[i])) {
                                    element.value += pasteData[i];
                                    break;
                                }
                                else {
                                    break;
                                }
                            }
                            else if (pasteData[i] == pattern[element.value.length]) {
                                element.value += pasteData[i];
                                break;
                            }
                            else {
                                element.value += pattern[element.value.length];
                            }
                        }
                    }

                    if (event.preventDefault) {
                        event.stopPropagation();
                        event.preventDefault();
                    }
                    break;
                }
        }
    };

    // - Formats the [element] value if possible and returns true if the [element] value matches the format of [pattern].
    input.formatCurrency = function (element, event, pattern) {        
        event = event || window.event;
        if (!element.originalValue) { element.originalValue = ""; }

        // - TODO: Improvements with oninput to handle delete/backspace

        switch (event.type) {
            case "focus":
                {
                    if (element.value == "") {
                        element.value = pattern;
                    }
                    break;
                }
            case "keypress":
                {
                    if (element.value == "") {
                        element.value = pattern;
                    }

                    var keyCode = event.which || event.keyCode;

                    if (input.isNotValue(keyCode)) {
                        return true;
                    }

                    var value = String.fromCharCode(keyCode);

                    if (isNaN(value)) {
                        return false;
                    }

                    element.originalValue = parseInt(element.value.replace(/\D/g, '') + value).toString();
                    element.formatValue = pattern;

                    var length = pattern.length;
                    var current = 0;
                    var offset = 0;

                    while (current < length) {
                        if (current - offset >= element.originalValue.length) { break; }

                        if (/^[0-9]$/.test(pattern[pattern.length - 1 - current])) {
                            element.formatValue = element.formatValue.substr(0, element.formatValue.length - 1 - current) + element.originalValue[element.originalValue.length - 1 - current + offset] + element.formatValue.substr(element.formatValue.length - 1 - current + element.originalValue[element.originalValue.length - 1 - current + offset].length);
                        }
                        else {
                            offset++;
                        }

                        current++;
                    }

                    if (element.originalValue.length + offset > length) {
                        for (var i = element.originalValue.length - 1 - current + offset; i >= 0; i--) {
                            element.formatValue = element.originalValue[i] + element.formatValue;
                        }
                    }

                    element.value = element.formatValue;
                    return false;
                }
            case "paste":
                {
                    var pasteData = event.clipboardData ? event.clipboardData.getData('text/plain') : "";
                    if (isNaN(pasteData)) {
                        if (event.preventDefault) {
                            event.stopPropagation();
                            event.preventDefault();
                        }
                        return false;
                    }

                    element.originalValue = pasteData;
                    element.formatValue = pattern;

                    var length = pattern.length;
                    var current = 0;
                    var offset = 0;

                    while (current < length) {
                        if (current - offset >= element.originalValue.length) { break; }

                        if (/^[0-9]$/.test(pattern[pattern.length - 1 - current])) {
                            element.formatValue = element.formatValue.substr(0, element.formatValue.length - 1 - current) + element.originalValue[element.originalValue.length - 1 - current + offset] + element.formatValue.substr(element.formatValue.length - 1 - current + element.originalValue[element.originalValue.length - 1 - current + offset].length);
                        }
                        else {
                            offset++;
                        }

                        current++;
                    }

                    if (element.originalValue.length + offset > length) {
                        for (var i = element.originalValue.length - 1 - current + offset; i >= 0; i--) {
                            element.formatValue = element.originalValue[i] + element.formatValue;
                        }
                    }

                    element.value = element.formatValue;

                    if (event.preventDefault) {
                        event.stopPropagation();
                        event.preventDefault();
                    }
                    break;
                }
        }
    };

    // - Returns true if the [keyCode] is a non-value code (ie. Enter, Escape, etc).
    input.isNotValue = function (keyCode) {
        if (keyCode == 8 || keyCode == 9 || keyCode == 13 || keyCode == 27 || (keyCode > 32 && keyCode < 41) || keyCode == 46) {
            return true;
        }
    };
})(window.input = window.input || {});

/*--------------- [ TEXT FUNCTIONS ] ---------------*/

/*--------------- [ ELEMENT/NAVIGATION FUNCTIONS ] ---------------*/

// - Returns the document element of [id].
function $(id) {
    return document.getElementById(id);
}

// - Returns a new element of [element] type.
function _$(element) {
    return document.createElement(element);
}

(function (nav) {
    // - Returns the next element (sibling in hierarchical order) after N matches.
    nav.next = function (element, matches) {
        var count = matches ? parseInt(matches) : 0;
        while (element = element.nextSibling) {
            if (element.nodeType != 1) { continue; }
            if (count > 0) { count--; continue; }            
            return element;
        }
        return false;
    };

    // - Returns the previous element (sibling in hierarchical order) after N matches.
    nav.prev = function (element, matches) {
        var count = matches ? parseInt(matches) : 0;
        while (element = element.previousSibling) {
            if (element.nodeType != 1) { continue; }
            if (count > 0) { count--; continue; }
            return element;
        }
        return false;
    };

    // - Returns the first element after N matches.
    nav.first = function (element, matches) {
        var count = matches ? parseInt(matches) : 0;
        for (var i = 0; i < element.childNodes.length; i++) {
            if (element.childNodes[i].nodeType != 1) { continue; }
            if (count > 0) { count--; continue; }
            return element.childNodes[i];
        }
        return false;
    };

    // - Returns the last element after N matches.
    nav.last = function (element, matches) {
        var count = matches ? parseInt(matches) : 0;
        for (var i = element.childNodes.length - 1; i >= 0; i--) {
            if (element.childNodes[i].nodeType != 1) { continue; }
            if (count > 0) { count--; continue; }
            return element.childNodes[i];
        }
        return false;
    };

    // - Returns the first parent with the specified [parentID].
    nav.findParentByID = function (element, parentID) {
        while (object = object.parentNode) {
            if (object.id == parentID) {
                return object;
            }
        }
        return false;
    };

    // - Returns the first parent by element name after N matches.
    nav.findParent = function (element, name, matches) {
        var count = matches ? parseInt(matches) : 0;
        while (element = element.parentNode) {
            if (element.nodeName.toUpperCase() != name) { continue; }
            if (count > 0) { count--; continue; }
            return element;
        }
        return false;
    };

    // - Returns the first child (recursively) by element name after N matches.
    nav.findChild = function (element, name, matches) {
        var count = matches ? parseInt(matches) : 0;
        for (var i = 0; i < element.childNodes.length; i++) {
            if (element.childNodes[i].nodeType != 1) { continue; }
            if (element.childNodes[i].nodeName.toUpperCase() == name) {
                if (count > 0) { count--; continue; }
                return element.childNodes[i];
            }
            else if (element.childNodes[i].hasChildNodes()) {
                var result = nav.findChild(element.childNodes[i], name, count);
                if (result) { return result; }
            }
        }
        return false;
    };

    // - Returns an array[2] of the position X and Y on screen of the object.
    nav.findPosition = function (element) {
        var left = 0, top = 0;
        if (element.offsetParent) {
            do {
                left += element.offsetLeft;
                top += element.offsetTop;
            } while (element = element.offsetParent);
        }
        return [left, top];
    };
    
    // - Returns a copy of a <template> tag.
    nav.getTemplate = function (templateID) {
        return document.importNode(document.getElementById(templateID).content, true);
    };
})(window.nav = window.nav || {});

/*--------------- [ VALIDATION FUNCTIONS ] ---------------*/

(function (form) {
    // - Validates a form element and returns true if valid.
    form.validate = function (element) {
        var errors = 0;
        for (var i = 0; i < element.length; i++) {
            errors += form.validateInput(element[i]) ? 0 : 1;
        }

        return errors == 0;
    };

    // - Validates a input element and returns true if valid.
    form.validateInput = function (element) {
        switch (element.type.toUpperCase()) {
            case "CHECKBOX":
                {
                    if (element.getAttribute("required") && element.checked == false) {
                        form.inputError(element, "required");
                        return false;
                    }
                    break;
                }
            case "RADIO":
                {
                    // - TODO: Improve, this checks once per radio button regardless if the group's been checked already.
                    if (input.getAttribute("required")) {
                        var isValid = false;
                        for (var i = 0; i < element.form.length; i++) {
                            if (!element.form[i].type.toUpperCase() == "RADIO" || element.form[i].name != element.name) { continue; }
                            if (element.form[i].checked) {
                                isValid = true;
                                break;
                            }
                        }

                        if (!isValid) {
                            form.inputError(element, "required");
                            return false;
                        }
                    }
                    break;
                }
            case "NUMBER":
                {
                    if (element.getAttribute("required") && element.value == "") {
                        form.inputError(element, "required");
                        return false;
                    }

                    if (element.getAttribute("min")) {
                        var min = parseInt(element.getAttribute("min"));
                        var value = parseInt(element.value);
                        if (isNaN(min) || isNaN(value) || value < min) {
                            form.inputError(element, "min");
                            return false;
                        }
                    }

                    if (element.getAttribute("max")) {
                        var max = parseInt(element.getAttribute("max"));
                        var value = parseInt(element.value);
                        if (isNaN(max) || isNaN(value) || value > max) {
                            form.inputError(element, "max");
                            return false;
                        }
                    }
                    break;
                }
            case "DATE":
                {
                    if (element.getAttribute("required") && element.value == "") {
                        form.inputError(element, "required");
                        return false;
                    }

                    if (element.getAttribute("min")) {
                        var min = Date.parse(element.getAttribute("min"));
                        var value = Date.parse(element.value);
                        if (isNaN(min) || isNaN(value) || value < min) {
                            form.inputError(element, "min");
                            return false;
                        }
                    }

                    if (element.getAttribute("max")) {
                        var max = Date.parse(element.getAttribute("max"));
                        var value = Date.parse(element.value);
                        if (isNaN(max) || isNaN(value) || value > max) {
                            form.inputError(element, "max");
                            return false;
                        }
                    }
                    break;
                }
            default:
                {
                    if (element.getAttribute("required") && element.value == "") {
                        form.inputError(element, "required");
                        return false;
                    }

                    if (element.getAttribute("pattern")) {
                        // - TODO: Test
                        var regex = new RegExp(element.getAttribute("pattern"));
                        if (!regex.test(element.value)) {
                            form.inputError(element, "pattern");
                            return false;
                        }
                    }

                    if (element.getAttribute("min")) {
                        var min = parseInt(element.getAttribute("min"));
                        if (isNaN(min) || element.value.length < min) {
                            form.inputError(element, "min");
                            return false;
                        }
                    }

                    if (element.getAttribute("max")) {
                        var max = parseInt(element.getAttribute("max"));
                        if (isNaN(max) || element.value.length < max) {
                            form.inputError(element, "max");
                            return false;
                        }
                    }
                    break;
                }            
        }

        if (element.getAttribute("validate")) {
            if (!eval(element.getAttribute("validate").replace("this", "element"))) {
                form.inputError(element, element.getAttribute("validate"));
                return false;
            }
        }

        form.inputValid(element);
        return true;
    };

    // - Calls any function set to the [onerror] attribute.
    form.inputError = function (element, type) {
        if (input.getAttribute("onerror") != null) {
            eval(input.getAttribute("onerror").replace("this", "element").replace("type", "'" + type + "'"));
        }
    };

    // - Calls any function set to the [onvalid] attribute.
    form.inputValid = function (element) {
        if (input.getAttribute("onvalid") != null) {
            eval(input.getAttribute("onvalid").replace("this", "element").replace("type", "'" + type + "'"));
        }
    };
})(window.form = window.form || {});

/*--------------- [ AJAX FUNCTIONS ] ---------------*/

(function (ajax) {
    // - Sends [data] (JSON) to the [url] and invokes [callback] with the response as parameter or "false" as parameter when the request fails.
    ajax.post = function (url, data, callback) {
        var request = new XMLHttpRequest();

        request.onreadystatechange = function () {
            if (request.readyState == 4) {
                if (request.status == 200) {
                    var response = request.responseText ? JSON.parse(request.responseText) : true;

                    callback(response);
                }
                else {
                    callback(false);
                }
            }
        }

        request.open("POST", url, true);
        request.setRequestHeader("Content-Type", "application/json");
        request.send(JSON.stringify(data));
    };

    // - Calls the [url] and invokes [callback] with the response as parameter or "false" as parameter when the request fails.
    ajax.get = function (url, callback) {
        var request = new XMLHttpRequest();

        request.onreadystatechange = function () {
            if (request.readyState == 4) {
                if (request.status == 200) {
                    var response = request.responseText ? JSON.parse(request.responseText) : true;

                    callback(response);
                }
                else {
                    callback();
                }
            }
        }

        request.open("GET", url, true);
        request.send();
    };

    // - Submits the [form] element to the [url] and invokes [callback] with the response as parameter or "false" as parameter when the request fails.
    ajax.submit = function (url, form, callback) {
        var request = new XMLHttpRequest();
        var data = new FormData(form);

        request.onreadystatechange = function () {
            if (request.readyState == 4) {
                if (request.status == 200) {
                    var response = request.responseText ? JSON.parse(request.responseText) : true;

                    callback(response);
                }
                else {
                    callback();
                }
            }
        }

        request.open("POST", url, true);
        request.send(data);
    };
})(window.ajax = window.ajax || {});