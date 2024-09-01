#region License
/*
MIT License

Copyright (c) 2023 Johnathan A Drews

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using HularionCore.Pattern.Functional;
using HularionPlugin.FileSystem.Request;
using HularionPlugin.FileSystem.Request.Directory;
using HularionPlugin.FileSystem.Request.File;
using HularionPlugin.FileSystem.Request.System;
using HularionPlugin.FileSystem.Response;
using HularionPlugin.FileSystem.Response.Directory;
using HularionPlugin.FileSystem.Response.File;
using HularionPlugin.FileSystem.Response.System;
using HularionPlugin.Route;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace HularionPlugin.FileSystem
{
    public class FileRouteProvider : IRouteProvider
    {
        public IEnumerable<HularionRoute> Routes { get; private set; }

        public string Purpose => "Provides routes for interacting with the file system.";

        public string Name => "FileRoute";

        public string Key => String.Format("Hularion.{0}", Name);

        private string baseRoute = "hularion/host/disk";
        private string directoryRoute;
        private string fileRoute;
        private string systemRoute;
        private string directoryDelimiter = @"\";

        private FileAttributes[] fileAttributes = Enum.GetValues<FileAttributes>();

        public FileRouteProvider()
        {

            var routes = new List<HularionRoute>();
            Routes = routes;

            directoryRoute = String.Format(@"{0}/directory", baseRoute);
            fileRoute = String.Format(@"{0}/file", baseRoute);
            systemRoute = String.Format(@"{0}/system", baseRoute);

            routes.Add(new HularionRoute<DelimiterRequest, DelimiterResponse>
            {
                Route = String.Format(@"{0}/delimiter", baseRoute),
                Name = "Get Delimiter",
                Method = "GetDelimiter",
                Usage = "Retrieves the delimiter used to separate file names. (e.g. '\' or '/')",
                Handler = ParameterizedFacade.FromSingle<RoutedRequest<DelimiterRequest>, RoutedResponse<DelimiterResponse>>(request 
                => new RoutedResponse<DelimiterResponse>()
                {
                    Detail = new DelimiterResponse() { Delimiter = directoryDelimiter }
                })
            });

            AddDirectoryRoutes(routes);
            AddFileRoutes(routes);
            AddSystemRoutes(routes);
        }

        private string MakeBaseRoute(string route)
        {
            return String.Format(@"{0}/{1}", baseRoute, route).ToLower();
        }

        private string MakeDirectoryRoute(string route)
        {
            return String.Format(@"{0}/{1}", directoryRoute, route).ToLower();
        }

        private string MakeFileRoute(string route)
        {
            return String.Format(@"{0}/{1}", fileRoute, route).ToLower();
        }

        private string MakeSystemRoute(string route)
        {
            return String.Format(@"{0}/{1}", systemRoute, route).ToLower();
        }

        private FileAttributes[] GetAttributes(FileAttributes attributes)
        {
            var result = new List<FileAttributes>();
            foreach (FileAttributes attribute in fileAttributes)
            {
                if((attributes & attribute) == attribute)
                {
                    result.Add(attribute);
                }
            }
            return result.ToArray();
        }

        private void AddDirectoryRoutes(List<HularionRoute> routes)
        {
            routes.Add(new HularionRoute<DirectoryAllAttributeRequest, DirectoryAllAttributeResponse>
            {
                Route = MakeDirectoryRoute("allAttributes"),
                Name = "Get All Possible Directory Attributes",
                Method = "GetPossibleDirectoryAttributes",
                Usage = "Returns all available attributes for directories.",
                Handler = ParameterizedFacade.FromSingle<RoutedRequest<DirectoryAllAttributeRequest>, RoutedResponse<DirectoryAllAttributeResponse>>(request =>
                {
                    var response = request.CreateResponse< DirectoryAllAttributeResponse>();
                    response.Detail.Attributes = Enum.GetNames<FileAttribute>();
                    response.State = RoutedResponseState.Success;
                    return response;
                })
            });

            routes.Add(new HularionRoute<DirectoryAttributeRequest, DirectoryAllAttributeResponse>
            {
                Route = MakeDirectoryRoute("attributes"),
                Name = "Get Directory's Attributes",
                Method = "GetDirectoryAttributes",
                Usage = "Returns attributes of the provided directory.",
                Handler = ParameterizedFacade.FromSingle<RoutedRequest<DirectoryAttributeRequest>, RoutedResponse<DirectoryAllAttributeResponse>>(request =>
                {
                    var response = request.CreateResponse< DirectoryAllAttributeResponse>();
                    FileAttributes fileAttributes;
                    try
                    {
                        fileAttributes = File.GetAttributes(request.Detail.Path);
                    }
                    catch (DirectoryNotFoundException e)
                    {
                        response.SetAsFailure(request.CreateErrorMessage(header: "Directory Not Found"));
                        return response;
                    }
                    catch (UnauthorizedAccessException e)
                    {
                        response.SetAsFailure(request.CreateErrorMessage(header: "Directory Access Unauthorized"));
                        return response;
                    }
                    catch (Exception e)
                    {
                        response.SetAsFailure(request.CreateErrorMessage(header: "Directory Access Exception"));
                        return response;
                    }
                    response.Detail.Attributes = GetAttributes(fileAttributes).Select(x => x.ToString());
                    response.State = RoutedResponseState.Success;
                    return response;
                })
            });

            routes.Add(new HularionRoute<DirectoryCopyRequest, DirectoryCopyResponse>
            {
                Route = MakeDirectoryRoute("copy"),
                Name = "Copy Directory",
                Method = "CopyDirectory",
                Usage = "Copies one or more directories from one location to another.",
                Handler = ParameterizedFacade.FromSingle<RoutedRequest<DirectoryCopyRequest>, RoutedResponse<DirectoryCopyResponse>>(request => 
                {
                    var response = new RoutedResponse<DirectoryCopyResponse>();
                    response.SetAsFailure(request.CreateErrorMessage(header: "Not Implemented - DirectoryCopyRequest"));
                    return response;
                })
            });

            routes.Add(new HularionRoute<DirectoryCreateRequest, DirectoryCreateResponse>
            {
                Route = MakeDirectoryRoute("create"),
                Name = "Create Directory",
                Method = "CreateDirectory",
                Usage = "",
                Handler = ParameterizedFacade.FromSingle<RoutedRequest<DirectoryCreateRequest>, RoutedResponse<DirectoryCreateResponse>>(request =>
                {
                    var response = new RoutedResponse<DirectoryCreateResponse>();
                    response.SetAsFailure(request.CreateErrorMessage(header: "Not Implemented - DirectoryCopyRequest"));
                    return response;
                })
            });

            routes.Add(new HularionRoute<DirectoryDeleteRequest, DirectoryDeleteResponse>
            {
                Route = MakeDirectoryRoute("delete"),
                Name = "Delete Directory",
                Method = "DeleteDirectory",
                Usage = "",
                Handler = ParameterizedFacade.FromSingle<RoutedRequest<DirectoryDeleteRequest>, RoutedResponse<DirectoryDeleteResponse>>(request =>
                {
                    var response = new RoutedResponse<DirectoryDeleteResponse>();
                    response.SetAsFailure(request.CreateErrorMessage(header: "Not Implemented - DirectoryCopyRequest"));
                    return response;
                })
            });

            routes.Add(new HularionRoute<DirectoryMoveRequest, DirectoryMoveResponse>
            {
                Route = MakeDirectoryRoute("move"),
                Name = "Move Directory",
                Method = "MoveDirectory",
                Usage = "",
                Handler = ParameterizedFacade.FromSingle<RoutedRequest<DirectoryMoveRequest>, RoutedResponse<DirectoryMoveResponse>>(request =>
                {
                    var response = new RoutedResponse<DirectoryMoveResponse>();
                    response.SetAsFailure(request.CreateErrorMessage(header: "Not Implemented - DirectoryCopyRequest"));
                    return response;
                })
            });

            routes.Add(new HularionRoute<DirectoryReadRequest, DirectoryReadResponse>
            {
                Route = MakeDirectoryRoute("read"),
                Name = "Read Directory",
                Method = "ReadDirectory",
                Usage = "Reads the directory, returning all subdirectories and files matching the provided attributes.",
                Handler = ParameterizedFacade.FromSingle<RoutedRequest<DirectoryReadRequest>, RoutedResponse<DirectoryReadResponse>>(request =>
                {                    
                    var response = request.CreateResponse<DirectoryReadResponse>();
                    try
                    {
                        var directories = Directory.EnumerateDirectories(request.Detail.Directory);
                        response.Detail.Directories = directories.Select(x => new DirectoryInformation(x)).ToList();

                        var files = Directory.EnumerateFiles(request.Detail.Directory);
                        response.Detail.Files = files.Select(x => new FileInformation(x)).ToList();
                    }
                    catch(UnauthorizedAccessException e)
                    {
                        response.SetAsFailure(new RoutedResponseMessage() { IsError = true, Type = RoutedResponseMessageType.Error, Header = "Access Denied", Message=$"Access to directory {request.Detail.Directory} was denied." });
                    }
                    catch(IOException e)
                    {
                        response.SetAsFailure(new RoutedResponseMessage() { IsError = true, Type = RoutedResponseMessageType.Error, Header = "IO Exception", Message= e.Message });
                    }
                    catch(Exception e)
                    {
                        response.SetAsFailure(new RoutedResponseMessage() { IsError = true, Header = "Unknown Error", Type = RoutedResponseMessageType.Error});
                    }
                    return response;
                })
            });

            routes.Add(new HularionRoute<DrivesRequest, DrivesResponse>
            {
                Route = MakeBaseRoute("drives"),
                Name = "Get Drives",
                Method = "GetDrives",
                Usage = "Returns the available logical drives.",
                Handler = ParameterizedFacade.FromSingle<RoutedRequest<DrivesRequest>, RoutedResponse<DrivesResponse>>(request =>
                {
                    var response = new RoutedResponse<DrivesResponse>();
                    response.Detail = new DrivesResponse();
                    response.Detail.AddDrives(DriveInfo.GetDrives());
                    return response;
                })
            });

        }

        private void AddFileRoutes(List<HularionRoute> routes)
        {
            routes.Add(new HularionRoute<FileAllAttributeRequest, FileAllAttributeResponse>
            {
                Route = MakeFileRoute("allAttributes"),
                Name = "Get All Possible File Attributes",
                Method = "GetPossibleFileAttributes",
                Usage = "Returns all available attributes of files.",
                Handler = ParameterizedFacade.FromSingle<RoutedRequest<FileAllAttributeRequest>, RoutedResponse<FileAllAttributeResponse>>(request =>
                {
                    var response = request.CreateResponse< FileAllAttributeResponse>();
                    response.Detail.Attributes = Enum.GetNames<FileAttribute>();
                    response.State = RoutedResponseState.Success;
                    return response;
                })
            });

            routes.Add(new HularionRoute<FileAttributeRequest, FileAttributeResponse>
            {
                Route = MakeFileRoute("attributes"),
                Name = "Get File's Attributes",
                Method = "GetFileAttributes",
                Usage = "Returns the attributes of the specified file",
                Handler = ParameterizedFacade.FromSingle<RoutedRequest<FileAttributeRequest>, RoutedResponse<FileAttributeResponse>>(request =>
                {
                    var response = request.CreateResponse< FileAttributeResponse>();
                    FileAttributes fileAttributes;
                    try
                    {
                        fileAttributes = File.GetAttributes(request.Detail.Path);
                    }
                    catch (FileNotFoundException e)
                    {
                        response.SetAsFailure(request.CreateErrorMessage(header: "File Not Found"));
                        return response;
                    }
                    catch (UnauthorizedAccessException e)
                    {
                        response.SetAsFailure(request.CreateErrorMessage(header: "File Access Unauthorized"));
                        return response;
                    }
                    catch (Exception e)
                    {
                        response.SetAsFailure(request.CreateErrorMessage(header: "File Access Exception"));
                        return response;
                    }
                    response.Detail.Attributes = GetAttributes(fileAttributes).Select(x => x.ToString());
                    response.State = RoutedResponseState.Success;
                    return response;
                })
            });

            routes.Add(new HularionRoute<FileCopyRequest, FileCopyResponse>
            {
                Route = MakeFileRoute("copy"),
                Name = "Copy File",
                Method = "CopyFile",
                Usage = "",
                Handler = ParameterizedFacade.FromSingle<RoutedRequest<FileCopyRequest>, RoutedResponse<FileCopyResponse>>(request =>
                {
                    var response = new RoutedResponse<FileCopyResponse>();
                    response.SetAsFailure(request.CreateErrorMessage(header: "Not Implemented - DirectoryCopyRequest"));
                    return response;
                })
            });

            routes.Add(new HularionRoute<FileCreateRequest, FileCreateResponse>
            {
                Route = MakeFileRoute("create"),
                Name = "Create File",
                Method = "CreateFile",
                Usage = "",
                Handler = ParameterizedFacade.FromSingle<RoutedRequest<FileCreateRequest>, RoutedResponse<FileCreateResponse>>(request =>
                {
                    var response = new RoutedResponse<FileCreateResponse>();
                    response.SetAsFailure(request.CreateErrorMessage(header: "Not Implemented - DirectoryCopyRequest"));
                    return response;
                })
            });

            routes.Add(new HularionRoute<FileDeleteRequest, FileDeleteResponse>
            {
                Route = MakeFileRoute("delete"),
                Name = "Delete File",
                Method = "DeleteFile",
                Usage = "",
                Handler = ParameterizedFacade.FromSingle<RoutedRequest<FileDeleteRequest>, RoutedResponse<FileDeleteResponse>>(request =>
                {
                    var response = new RoutedResponse<FileDeleteResponse>();
                    response.SetAsFailure(request.CreateErrorMessage(header: "Not Implemented - DirectoryCopyRequest"));
                    return response;
                })
            });

            routes.Add(new HularionRoute<FileMoveRequest, FileMoveResponse>
            {
                Route = MakeFileRoute("move"),
                Name = "Move File",
                Method = "MoveFile",
                Usage = "",
                Handler = ParameterizedFacade.FromSingle<RoutedRequest<FileMoveRequest>, RoutedResponse<FileMoveResponse>>(request =>
                {
                    var response = new RoutedResponse<FileMoveResponse>();
                    response.SetAsFailure(request.CreateErrorMessage(header: "Not Implemented - DirectoryCopyRequest"));
                    return response;
                })
            });

            routes.Add(new HularionRoute<FileReadRequest, FileReadResponse>
            {
                Route = MakeFileRoute("read"),
                Name = "Read File",
                Method = "ReadFile",
                Usage = "",
                Handler = ParameterizedFacade.FromSingle<RoutedRequest<FileReadRequest>, RoutedResponse<FileReadResponse>>(request =>
                {
                    var response = new RoutedResponse<FileReadResponse>();
                    response.SetAsFailure(request.CreateErrorMessage(header: "Not Implemented - DirectoryCopyRequest"));
                    return response;
                })
            });

        }

        private void AddSystemRoutes(List<HularionRoute> routes)
        {
            routes.Add(new HularionRoute<OpenFolderRequest, OpenFolderResponse>
            {
                Route = MakeSystemRoute("openfolder"),
                Name = "Open Folder",
                Method = "OpenFolder",
                Usage = "Opens the folder using the operating system's viewer.",
                Handler = ParameterizedFacade.FromSingle<RoutedRequest<OpenFolderRequest>, RoutedResponse<OpenFolderResponse>>(request =>
                {
                    var response = request.CreateResponse<OpenFolderResponse>();
                    var directory = request.Detail.Directory;
                    if (!Directory.Exists(request.Detail.Directory))
                    {
                        if (File.Exists(request.Detail.Directory) && request.Detail.Directory.Contains(directoryDelimiter))
                        {
                            directory = request.Detail.Directory.Substring(0, request.Detail.Directory.LastIndexOf(directoryDelimiter));
                        }
                    }
                    if (!Directory.Exists(directory))
                    {
                        response.IsFailure = true;
                        response.Messages.Add(new RoutedResponseMessage() { IsError = true, Header="Folder not found.", Message = "The requested folder was not found", Type = RoutedResponseMessageType.Error });
                        return response;
                    }
                    Process.Start(new ProcessStartInfo()
                    {
                        FileName = "explorer.exe",
                        Arguments = directory
                    });
                    response.State = RoutedResponseState.Success;
                    return response;
                })
            });
        }

    }
}
