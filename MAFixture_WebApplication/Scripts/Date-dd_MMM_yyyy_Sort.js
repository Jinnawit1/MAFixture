/**
 * This sorting plug-in will sort, in calendar order, data which
 * is in the format "MMM yyyy" or "MMMM yyyy". Inspired by forum discussion:
 * http://datatables.net/forums/discussion/1242/sorting-dates-with-only-month-and-year
 *
 * Please note that this plug-in is **deprecated*. The
 * [datetime](//datatables.net/blog/2014-12-18) plug-in provides enhanced
 * functionality and flexibility.
 *
 *  @name Date (MMM yyyy) or (MMMM yyyy)
 *  @anchor Sort dates in the format `MMM yyyy` or `MMMM yyyy`
 *  @author Phil Hurwitz
 *  @deprecated
 *
 *  @example
 *    $('#example').DataTable( {
 *       columnDefs: [
 *         { type: 'stringMonthYear', targets: 0 }
 *       ]
 *    } );
 */

jQuery.extend(jQuery.fn.dataTableExt.oSort, {
    "stringMonthYear-pre": function (s) {
        s = s.replace("<div style=\"background-color: yellow;font-weight: bold;\">", "");
        s = s.replace("</div>", "");
        if (jQuery.trim(s) != "") {
            var months = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];

            var dateComponents = s.split(" ");
            dateComponents[1] = dateComponents[1].replace(",", "");
            dateComponents[2] = jQuery.trim(dateComponents[2]);

            var year = dateComponents[2];

            var month = 0;
            for (var i = 0; i < months.length; i++) {
                if (months[i].toLowerCase() == dateComponents[1].toLowerCase().substring(0, 3)) {
                    month = i;
                    break;
                }
            }

            return new Date(year, month, dateComponents[0]);
        }
        else
        {
            return new Date(8640000000000000);
        }
    },

    "stringMonthYear-asc": function (a, b) {
        return ((a < b) ? -1 : ((a > b) ? 1 : 0));
    },

    "stringMonthYear-desc": function (a, b) {
        return ((a < b) ? 1 : ((a > b) ? -1 : 0));
    }
});
