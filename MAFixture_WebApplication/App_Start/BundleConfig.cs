using System.Web;
using System.Web.Optimization;

namespace MAFixture_WebApplication.App_Start
{
    public class BundleConfig
    {
         public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/boxicons.css",
                //"~/Content/bootstrap.css",
                "~/Content/bootstrap.min.css",
                "~/Content/daterangepicker.css",
                "~/Content/bootstrap-datepicker3.css",
                "~/Content/bootstrap-tagsinput.css",
                //"~/Content/jquery.timepicker.min.css",
                //"~/Content/bootstrap-datepicker3.css",
                "~/Content/DataTables/css/*.css",
                //"~/Content/spinkit.css",
                //"~/Content/spinners/*.css",
                //"~/Content/please-wait.css",
                "~/fontawesome-free-5.11.2-web/css/all.css",
                "~/fontawesome-free-5.11.2-web/css/v4-shims.css",
                "~/Content/site.css",
                "~/node_modules/vis-timeline/styles/vis-timeline-graph2d.min.css",
                "~/Content/buttons.dataTables.min.css",
                "~/Scripts/select2/dist/css/select2.min.css"
            ));

            bundles.Add(new Bundle("~/bundles/bootstrap4").Include(
                "~/Scripts/bootstrap.js",
                "~/Scripts/daterangepicker.js",
                "~/Scripts/bootstrap.min.js",
                "~/Scripts/buttons.html5.min.js",
                "~/Scripts/buttons.print.min.js",
                "~/Scripts/dataTables.buttons.min.js",
                "~/Scripts/jszip.min.js",
                //"~/Scripts/pdfmake.min.js",
                "~/Scripts/vfs_fonts.js",
                //"~/Scripts/jspdf.umd.min.js",
                "~/Scripts/html2canvas.min.js",
                "~/Scripts/chart.js",
                "~/Scripts/chartjs-plugin-datalabels.min.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                "~/node_modules/vis-timeline/standalone/umd/vis-timeline-graph2d.min.js",
                "~/Scripts/standard-function.js",
                "~/Scripts/jquery-3.3.1.min.js",
                "~/Scripts/moment.min.js",
                //"~/Scripts/pdf.js",
                //"~/Scripts/boostrap3/bootstrap.min.js",
                "~/Scripts/SheetJS/*.js",
                "~/Scripts/DataTables/jquery.dataTables.min.js",
                "~/Scripts/dataTables.min.js",
                "~/Scripts/dataTables.select.min.js",
                //"~/Scripts/boostrap3/bootstrap.js",
                "~/Scripts/bootstrap-tagsinput.min.js",
                "~/Scripts/bootstrap3-typeahead.min.js",
                "~/Scripts/respond.js",
                //"~/Scripts/modernizr-2.6.2.js",
                "~/Scripts/jquery-ui-1.12.0.js",
                //"~/Scripts/jquery.timepicker.min.js",
                "~/Scripts/bootstrap-datepicker.min.js",

                //"~/Scripts/please-wait.min.js",
                "~/Scripts/date.format.js",
                //"~/Scripts/any-number.js",
                "~/Scripts/Date-dd_mmm_yyyy_hhMMss_Sort.js",
                "~/fontawesome-free-5.11.2-web/js/all.min.js",
                "~/fontawesome-free-5.11.2-web/js/v4-shims.min.js",
                "~/Scripts/DataTables/dataTables.rowsGroup.js",
                "~/Scripts/DataTables/dataTables.fixedColumns.js",
                /*"~/Scripts/DataTables/dataTables.rowGroup.min.js",*/
                "~/Scripts/DataTables/dataTables.buttons.min.js",
                "~/Scripts/DataTables/buttons.flash.min.js",
                //"~/Scripts/DataTables/pdfmake.min.js",
                //"~/Scripts/DataTables/vfs_fonts.js",
                "~/Scripts/DataTables/buttons.html5.min.js",
                "~/Scripts/DataTables/jszip.min.js",
                "~/Scripts/DataTables/dataTables.fixedHeader.min.js",
                "~/Scripts/DataTables/buttons.colVis.min.js",
                "~/Scripts/index.min.js",
                "~/Scripts/indexImage.min.js",
                "~/Scripts/select2/dist/js/select2.min.js",
                "~/Scripts/heic2any.min.js"
            ));
        }
    }
}