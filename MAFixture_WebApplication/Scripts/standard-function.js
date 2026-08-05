function detectIE() {
    var ua = window.navigator.userAgent;

    // Test values; Uncomment to check result …

    // IE 10
    // ua = 'Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; Trident/6.0)';

    // IE 11
    // ua = 'Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko';

    // IE 12 / Spartan
    // ua = 'Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/39.0.2171.71 Safari/537.36 Edge/12.0';

    // Edge (IE 12+)
    // ua = 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2486.0 Safari/537.36 Edge/13.10586';

    var msie = ua.indexOf('MSIE ');
    if (msie > 0) {
        // IE 10 or older => return version number
        return parseInt(ua.substring(msie + 5, ua.indexOf('.', msie)), 10);
    }

    var trident = ua.indexOf('Trident/');
    if (trident > 0) {
        // IE 11 => return version number
        var rv = ua.indexOf('rv:');
        return parseInt(ua.substring(rv + 3, ua.indexOf('.', rv)), 10);
    }

    var edge = ua.indexOf('Edge/');
    if (edge > 0) {
        // Edge (IE 12+) => return version number
        return parseInt(ua.substring(edge + 5, ua.indexOf('.', edge)), 10);
    }

    // other browser
    return false;
}

//function copyToClipboard() {
//    var copyText = document.getElementById("drivePath");

//    // Select the text field
//    copyText.select();
//    copyText.setSelectionRange(0, 99999); // For mobile devices

//    // Copy the text inside the text field
//    navigator.clipboard.writeText(copyText.value);
//}

function copyToClipboard() {
    var copyText = document.getElementById("drivePath");

    // Select the text field
    copyText.select();
    copyText.setSelectionRange(0, 99999); // For mobile devices

    if (window.isSecureContext && navigator.clipboard)
    {
        // Copy the text inside the text field
        navigator.clipboard.writeText(copyText.value);
    }
    else
    {
        try
        {
            document.execCommand('copy');
        }
        catch (err)
        {
            
        }
    }
}

function disable_ModalFolder(chk) {
    var state_check = chk.checked;
    if (state_check) {
        if (getCookie("CarParOnline_user_ModalFolder_disable") == null) {
            setCookie("CarParOnline_user_ModalFolder_disable", "true", 1);
        }
    }
    else {
        if (getCookie("CarParOnline_user_ModalFolder_disable") != null) {
            deleteCookie("CarParOnline_user_ModalFolder_disable");
        }
    }
}

function setCookie(name, value, daysToLive) {
    // Encode value in order to escape semicolons, commas, and whitespace
    var cookie = name + "=" + encodeURIComponent(value);

    if (typeof daysToLive === "number") {
        /* Sets the max-age attribute so that the cookie expires
        after the specified number of days */
        cookie += "; max-age=" + (daysToLive * 24 * 60 * 60);

        document.cookie = cookie;
    }
}

function getCookie(name) {
    // Split cookie string and get all individual name=value pairs in an array
    var cookieArr = document.cookie.split(";");

    // Loop through the array elements
    for (var i = 0; i < cookieArr.length; i++) {
        var cookiePair = cookieArr[i].split("=");

        /* Removing whitespace at the beginning of the cookie name
        and compare it with the given string */
        if (name == cookiePair[0].trim()) {
            // Decode the cookie value and return
            return decodeURIComponent(cookiePair[1]);
        }
    }

    // Return null if not found
    return null;
}

function checkCookie(name) {
    var firstName = getCookie(name);

    if (firstName != "") {
        return true;
    }
    else {
        return false;
    }
}

function deleteCookie(name) {
    document.cookie = name + "=; max-age=0";
}


function AddBusinessDays(date, days) {
    if (days < 0) {
        return date;
    }

    if (days == 0) return date;

    var result = new Date(date);

    // date.DayOfWeek == DayOfWeek.Saturday
    if (result.getDay() == 6) {
        result.setDate(result.getDate() + 2);
        days -= 1;
    }
    // date.DayOfWeek == DayOfWeek.Sunday
    else if (date.getDay() == 0) {
        result.setDate(result.getDate() + 1);
        days -= 1;
    }

    // date = date.AddDays(days / 5 * 7);
    result.setDate(result.getDate() + ((days / 5) * 7));

    var extraDays = days % 5;
    if ((result.getDay() + extraDays) > 5) {
        extraDays += 2;
    }

    result.setDate(result.getDate() + extraDays);

    return result;
}

function GetBusinessDays(start, end) {

    var iStart = new Date(start.toDateString());
    var iEnd = new Date(end.toDateString());

    // start.DayOfWeek == DayOfWeek.Saturday
    if (iStart.getDay() == 6) {
        iStart.setDate(iStart.getDate() + 2);
    }
    // start.DayOfWeek == DayOfWeek.Sunday
    else if (iStart.getDay() == 0) {
        iStart.setDate(iStart.getDate() + 1);
    }

    // end.DayOfWeek == DayOfWeek.Saturday
    if (iEnd.getDay() == 6) {
        iEnd.setDate(iEnd.getDate() - 1);
    }
    // end.DayOfWeek == end.Sunday
    else if (iEnd.getDay() == 0) {
        iEnd.setDate(iEnd.getDate() - 2);
    }

    // int diff = (int)end.Subtract(start).TotalDays;
    var diff = (iEnd - iStart) / (1000 * 60 * 60 * 24);

    var result = ((diff / 7) * 5) + (diff % 7);

    if (iEnd.getDay() < iStart.getDay()) {
        return Math.floor(result - 2);
    }
    else {
        return Math.floor(result);
    }
}