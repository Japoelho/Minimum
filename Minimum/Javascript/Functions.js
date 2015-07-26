/*--------------- [ AJAX FUNCTIONS ] ---------------*/

// - Calls the [url] link with the [postData] as JSON post string, and calls [callback] passing the JSON parsed response as parameter on success or no parameter on failure.
function AjaxPost(url, postData, callback) {
    var ajax = new XMLHttpRequest();

    ajax.onreadystatechange = function () {
        if (ajax.readyState == 4) {
            if (ajax.status == 200) {
                var response = JSON.parse(ajax.responseText);

                callback(response);
            }
            else {
                callback();
            }
        }
    }

    ajax.open("POST", url, true);
    ajax.setRequestHeader('Content-Type', 'application/json');
    ajax.send(JSON.stringify(postData));
}

/*--------------- [ INPUT EVENT FUNCTIONS ] ---------------*/

// [onkeypress] [onpaste] 
// - Returns true if the event is numeric or only numeric values.
function isNumeric(event) {
    switch (event.type) {
        case "keypress":
            {
                var keyCode = event ? event.keyCode ? event.keyCode : event.which : window.event.keyCode ? window.event.keyCode : window.event.which;

                if (keyCode == 8 || keyCode == 9 || keyCode == 13 || keyCode == 27 || keyCode == 37 || keyCode == 38 || keyCode == 39 || keyCode == 40) {
                    return true;
                }

                if (isNaN(String.fromCharCode(keyCode))) {
                    return false;
                }
                else {
                    return true;
                }
            }
        case "paste":
            {
                var pasteData = event ? event.clipboardData ? event.clipboardData.getData('text/plain') : "" : window.event.clipboardData ? window.event.clipboardData.getData('text/plain') : "";

                if (isNaN(pasteData)) {
                    return false;
                }
                else {
                    return true;
                }

                if (event.preventDefault) {
                    event.stopPropagation();
                    event.preventDefault();
                }
                break;
            }
    }
}

// [onkeypress]
// - Returns true if the event matches the regex pattern.
function regexTest(event, pattern) {
    var keyCode = event ? event.keyCode ? event.keyCode : event.which : window.event.keyCode ? window.event.keyCode : window.event.which;

    if (keyCode == 8 || keyCode == 9 || keyCode == 13 || keyCode == 27 || keyCode == 37 || keyCode == 38 || keyCode == 39 || keyCode == 40) {
        return true;
    }

    var regExp = new RegExp(pattern);
    return regExp.test(String.fromCharCode(keyCode));
}

// [onkeypress] [onpaste]
// - Returns the format value of the input by the specified pattern, ex. '000.000.000-00' or 'AA-000'
function formatValue(input, event, pattern) {
    switch (event.type) {
        case "keypress":
            {
                var keyCode = event ? event.keyCode ? event.keyCode : event.which : window.event.keyCode ? window.event.keyCode : window.event.which;

                if (keyCode == 8 || keyCode == 9 || keyCode == 13 || keyCode == 27 || keyCode == 37 || keyCode == 38 || keyCode == 39 || keyCode == 40) {
                    return true;
                }

                while (true) {
                    if (input.value.length >= pattern.length) {
                        return false;
                    }
                    else if (/^[0-9]$/.test(pattern[input.value.length])) { //If numeric
                        if (!isNaN(String.fromCharCode(keyCode))) {
                            return true;
                        }
                        else {
                            return false;
                        }
                    }
                    else if (/^[a-zA-Z]$/.test(pattern[input.value.length])) { //If character only
                        if (/^[a-zA-Z]$/.test(String.fromCharCode(keyCode))) {
                            return true;
                        }
                        else {
                            return false;
                        }
                    }
                    else if (String.fromCharCode(keyCode) == pattern[input.value.length]) {
                        return true;
                    }
                    else {
                        input.value += pattern[input.value.length];
                    }
                }
                break;
            }
        case "paste":
            {
                var pasteData = event ? event.clipboardData ? event.clipboardData.getData('text/plain') : "" : window.event.clipboardData ? window.event.clipboardData.getData('text/plain') : "";
                for (var i = 0; i < pasteData.length; i++) {
                    while (true) {
                        if (input.value.length >= pattern.length) {
                            break;
                        }
                        else if (/^[0-9]$/.test(pattern[input.value.length])) { //If numeric
                            if (!isNaN(pasteData[i])) {
                                input.value += pasteData[i];
                                break;
                            }
                            else {
                                break;
                            }
                        }
                        else if (/^[a-zA-Z]$/.test(pattern[input.value.length])) { //If character only
                            if (/^[a-zA-Z]$/.test(pasteData[i])) {
                                input.value += pasteData[i];
                                break;
                            }
                            else {
                                break;
                            }
                        }
                        else if (pasteData[i] == pattern[input.value.length]) {
                            input.value += pasteData[i];
                            break;
                        }
                        else {
                            input.value += pattern[input.value.length];
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
}

// [onfocus] [onkeypress] [onpaste]
// - Returns the format numeric value of the input by the specified pattern, ex. '0,00'
function formatCurrency(input, event, pattern) {
    if (!input.originalValue) { input.originalValue = ""; }

    switch (event.type) {
        case "focus":
            {
                if (input.value == "") {
                    input.value = pattern;
                }
                break;
            }
        case "keypress":
            {
                if (input.value == "") {
                    input.value = pattern;
                }

                var keyCode = event ? event.keyCode ? event.keyCode : event.which : window.event.keyCode ? window.event.keyCode : window.event.which;

                if (keyCode == 8 || keyCode == 9 || keyCode == 13 || keyCode == 27 || keyCode == 37 || keyCode == 38 || keyCode == 39 || keyCode == 40) {
                    return true;
                }

                var value = String.fromCharCode(keyCode);

                if (isNaN(value)) {
                    return false;
                }

                input.originalValue = parseInt(input.value.replace(/\D/g, '') + value).toString();
                input.formatValue = pattern;

                var length = pattern.length;
                var current = 0;
                var offset = 0;

                while (current < length) {
                    if (current - offset >= input.originalValue.length) { break; }

                    if (/^[0-9]$/.test(pattern[pattern.length - 1 - current])) {
                        input.formatValue = replaceAt(input.formatValue, input.formatValue.length - 1 - current, input.originalValue[input.originalValue.length - 1 - current + offset]);
                    }
                    else {
                        offset++;
                    }

                    current++;
                }

                if (input.originalValue.length + offset > length) {
                    for (var i = input.originalValue.length - 1 - current + offset; i >= 0; i--) {
                        input.formatValue = input.originalValue[i] + input.formatValue;
                    }
                }

                input.value = input.formatValue;
                return false;
            }
        case "paste":
            {
                var pasteData = event ? event.clipboardData ? event.clipboardData.getData('text/plain') : "" : window.event.clipboardData ? window.event.clipboardData.getData('text/plain') : "";
                if (isNaN(pasteData)) {
                    if (event.preventDefault) {
                        event.stopPropagation();
                        event.preventDefault();
                    }
                    return false;
                }

                input.originalValue = pasteData;
                input.formatValue = pattern;

                var length = pattern.length;
                var current = 0;
                var offset = 0;

                while (current < length) {
                    if (current - offset >= input.originalValue.length) { break; }

                    if (/^[0-9]$/.test(pattern[pattern.length - 1 - current])) {
                        input.formatValue = replaceAt(input.formatValue, input.formatValue.length - 1 - current, input.originalValue[input.originalValue.length - 1 - current + offset]);
                    }
                    else {
                        offset++;
                    }

                    current++;
                }

                if (input.originalValue.length + offset > length) {
                    for (var i = input.originalValue.length - 1 - current + offset; i >= 0; i--) {
                        input.formatValue = input.originalValue[i] + input.formatValue;
                    }
                }

                input.value = input.formatValue;

                if (event.preventDefault) {
                    event.stopPropagation();
                    event.preventDefault();
                }
                break;
            }
    }
}

/*--------------- [ TEXT FUNCTIONS ] ---------------*/

// - Removes leading spaces from the string value.
function trim(value) {
    return value.replace(/^\s+|\s+$/g, "");
}

// - Replaces the character at the specific index of the original string.
function replaceAt(original, index, character) {
    return original.substr(0, index) + character + original.substr(index + character.length)
}

/*--------------- [ SCREEN EFFECTS ] ---------------*/

// - Creates a semi-transparent layer of the [scripts-background] CSS class, removes the layer if [removeOnClick] is set to true, and calls [onRemove] if specified.
function backgroundMask(removeOnClick, onRemove) {
    var background = document.createElement("div");
    background.className = "scripts-background";

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

/*--------------- [ ELEMENT FUNCTIONS ] ---------------*/

// - Returns the next element (sibling in hierarchical order) after N matches.
function nextElement(object, matches) {
    var countMatch = 0;
    if (matches) { countMatch = parseInt(matches); }

    var nextObject = object;
    while (nextObject = nextObject.nextSibling) {
        if (nextObject.nodeType == 1) {
            if (countMatch == 0) {
                return nextObject;
            }
            else {
                countMatch--;
            }
        }
    }
}

// - Returns the previous element (sibling in hierarchical order) after N matches.
function prevElement(object, matches) {
    var countMatch = 0;
    if (matches) { countMatch = parseInt(matches); }

    var prevObject = object;
    while (prevObject = prevObject.previousSibling) {
        if (prevObject.nodeType == 1) {
            if (countMatch == 0) {
                return prevObject;
            }
            else {
                countMatch--;
            }
        }
    }
}

// - Returns the first parent with the specified [parentID].
function findParentByID(object, parentID) {
    while (object = object.parentNode) {
        if (object.id == parentID) {
            return object;
        }
    }
    return false;
}

// - Returns the first parent by element name after N matches.
function findParent(object, parentName, matches) {
    var countMatch = 0;
    if (matches) { countMatch = parseInt(matches); }
    while (object = object.parentNode) {
        if (object.nodeName.toUpperCase() == parentName) {
            if (countMatch == 0) {
                return object;
            }
            else {
                countMatch--;
            }
        }
    }
    return false;
}

// - Returns the first child (recursively) by element name after N matches.
function findChild(object, childName, matches) {
    var countMatch = 0;
    if (matches) { countMatch = parseInt(matches); }
    for (var i = 0; i < object.childNodes.length; i++) {
        if (object.childNodes[i].nodeType == 1) {
            if (object.childNodes[i].nodeName.toUpperCase() == childName) {
                if (countMatch == 0) {
                    return object.childNodes[i];
                }
                else {
                    countMatch--;
                }
            }

            if (object.childNodes[i].hasChildNodes()) {
                var child = findChild(object.childNodes[i], childName, countMatch);
                if (child) {
                    return child;
                }
            }
        }
    }

    return false;
}

// - Returns an array[2] of the position X and Y on screen of the object.
function findPosition(object) {
    var posLeft = posTop = 0;
    if (object.offsetParent) {
        do {
            posLeft += object.offsetLeft;
            posTop += object.offsetTop;
        } while (object = object.offsetParent);
    }
    return [posLeft, posTop];
}

/*--------------- [ VALIDATION FUNCTIONS ] ---------------*/

// [onsubmit]
// - Validates the form and returns true if valid. This function calls [validateField] on all it's inputs. Optional attributes:
// [preSubmit]: Calls a function before the submit.
function validateForm(formObject) {
    var errorCount = 0;
    for (var i = 0; i < formObject.length; i++) {
        errorCount += validateField(formObject[i]) ? 0 : 1;
    }

    if (errorCount == 0 && formObject.getAttribute("preSubmit") != null) {
        eval(formObject.getAttribute("preSubmit"));
    }

    return errorCount > 0 ? false : true;
}

// [any event]
// - Validates the input and returns true if valid. Optional attributes:
// [required]: Specifies that the field requires a value.
// [min-length]: Specifies the minimum length of the value.
// [max-length]: Specifies the maximum length of the value.
// [test]: Tests the value against the specified regex pattern.
// [onvalidation]: Calls the specified function (expects a return) for custom validation.
// [on-correct]: Calls the specified function if the field is valid.
// [on-error]: Calls the specified function if the field is invalid.
function validateField(input) {
    switch (input.type.toUpperCase()) {
        case "CHECKBOX":
            {
                if (input.getAttribute("required") != null && input.checked == false) {
                    fieldError(input, "required");
                    return false;
                }
                break;
            }
        case "RADIO":
            {
                if (input.getAttribute("required") != null) {
                    var radios = document.getElementsByName(input.name);
                    var radioIsValid = false;
                    for (var i = 0; i < radios.length; i++) {
                        if (radios[i].checked == true) {
                            radioIsValid = true;
                            break;
                        }
                    }

                    if (radioIsValid == false) {
                        fieldError(input, "required");
                        return false;
                    }
                }
                break;
            }
        default:
            {
                if (input.getAttribute("required") != null && trim(input.value) == "") {
                    fieldError(input, "required");
                    return false;
                }

                if (input.getAttribute("test") != null && fieldTest(input) == false) {
                    fieldError(input, "test");
                    return false;
                }

                if (input.getAttribute("min-length") != null && input.value.length < parseInt(input.getAttribute("min-length"))) {
                    fieldError(input, "min-length");
                    return false;
                }

                if (input.getAttribute("max-length") != null && input.value.length < parseInt(input.getAttribute("max-length"))) {
                    fieldError(input, "max-length");
                    return false;
                }
                break;
            }
    }

    if (input.getAttribute("onvalidation") != null) {
        if (!eval(input.getAttribute("onvalidation").replace("this", "input"))) {
            fieldError(input, input.getAttribute("onvalidation"));
            return false;
        }
    }

    fieldValid(input);
    return true;
}

// - Calls the function specified on the [on-error] attribute.
function fieldError(input, errorType) {
    if (input.getAttribute("on-error") != null) {
        eval(input.getAttribute("on-error").replace("this", "input").replace("type", "'" + errorType + "'"));
    }
}

// - Calls the function specified on the [on-correct] attribute.
function fieldValid(input) {
    if (input.getAttribute("on-correct") != null) {
        eval(input.getAttribute("on-correct").replace("this", "input"));
    }
}

// - Returns the test of the regex pattern in the [test] attribute.
function fieldTest(input) {
    var regExp = new RegExp(input.getAttribute("test"));

    return regExp.test(input.value);
}