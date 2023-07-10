using System.Net;

namespace App.ExtendsMethods;
public static class AppExtends
{
    public static void AppStatusCodePage(this IApplicationBuilder app)
    {
        app.UseStatusCodePages(appError =>
        {
            appError.Run(async context =>
            {
                var response = context.Response;
                var code = response.StatusCode;
                
                var content = @$"
        <h1 style =color: red> co loi  xay ra {(HttpStatusCode)code}
        ";
                await response.WriteAsync(content);

            });
        });
    }
}