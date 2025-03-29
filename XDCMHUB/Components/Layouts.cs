using Spectre.Console;

namespace XDCMHUB.Components;

public class Layouts
{
    public static Layout LoginLayout()
    {
        return new Layout("Login");
    }

    public static Layout MainLayout()
    {
        return new Layout("Main")
            .SplitColumns(
                new Layout("Left")
                    .Size(30)
                    .SplitRows(
                        new Layout("OtherUserArea"),
                        new Layout("Others").Invisible()
                    ),
                new Layout("ChatArea")
            );
    }
}
