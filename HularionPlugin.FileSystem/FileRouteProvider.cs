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
using HularionCore.Pattern.Topology;
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
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
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
            // {0}/allAttributes
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

            // {0}/attributes
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

            // {0}/copy
            routes.Add(new HularionRoute<DirectoryCopyRequest, DirectoryCopyResponse>
            {
                Route = MakeDirectoryRoute("copy"),
                Name = "Copy Directory",
                Method = "CopyDirectory",
                Usage = "Copies one or more directories from one location to another, including all contents.\n"
                        + "May result in an incomplete copy if an error occurs.",
                Handler = ParameterizedFacade.FromSingle<RoutedRequest<DirectoryCopyRequest>, RoutedResponse<DirectoryCopyResponse>>(request => 
                {
                    var response = new RoutedResponse<DirectoryCopyResponse>();
                    try
                    {
                        if (String.IsNullOrWhiteSpace(request.Detail.SourceDirectory) || !request.Detail.SourceDirectory.Contains(@"\"))
                        {
                            response.SetAsFailure(request.CreateErrorMessage(header: "Invalid Source Directory"));
                            return response;
                        }
                        if (String.IsNullOrWhiteSpace(request.Detail.SourceDirectory) || !request.Detail.SourceDirectory.Contains(@"\"))
                        {
                            response.SetAsFailure(request.CreateErrorMessage(header: "Invalid Destination Directory"));
                            return response;
                        }

                        var directoryName = request.Detail.SourceDirectory.Substring(request.Detail.SourceDirectory.LastIndexOf('\\') + 1);
                        var destination = String.Format(@"{0}\{1}", request.Detail.DestinationPath.Trim().Trim('\\'), directoryName);
                        if(Directory.Exists(destination))
                        {
                            response.SetAsFailure(request.CreateErrorMessage(header: "Destination Exists Already"));
                            return response;
                        }

                        Directory.CreateDirectory(destination);

                        var traverser = new TreeTraverser<string>();
                        var plan = traverser.CreateEvaluationPlan(TreeTraversalOrder.ParentRightLeft, request.Detail.SourceDirectory, node =>
                        {
                            return Directory.GetDirectories(node);
                        }, true);
                        foreach (var directory in plan)
                        {
                            var files = Directory.GetFiles(directory);
                            var newDirectory = String.Format(@"{0}\{1}", destination, directory.Substring(request.Detail.SourceDirectory.Length).Trim().Trim('\\'));
                            Directory.CreateDirectory(newDirectory);
                            foreach (var file in files)
                            {
                                File.Copy(file, String.Format(@"{0}\{1}", newDirectory, file.Substring(file.LastIndexOf(@"\") + 1)));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        response.SetAsFailure(request.CreateErrorMessage(header: "Exception", message: ex.ToString()));
                        return response;
                    }
                    response.SetAsFailure(request.CreateErrorMessage(header: "Not Implemented - DirectoryCopyRequest"));
                    return response;
                })
            });

            // {0}/create
            routes.Add(new HularionRoute<DirectoryCreateRequest, DirectoryCreateResponse>
            {
                Route = MakeDirectoryRoute("create"),
                Name = "Create Directory",
                Method = "CreateDirectory",
                Usage = "",
                Handler = ParameterizedFacade.FromSingle<RoutedRequest<DirectoryCreateRequest>, RoutedResponse<DirectoryCreateResponse>>(request =>
                {
                    var response = new RoutedResponse<DirectoryCreateResponse>();
                    try
                    {
                        Directory.CreateDirectory(request.Detail.Directory);
                    }
                    catch (Exception ex)
                    {
                        response.SetAsFailure(request.CreateErrorMessage(header: "Exception", message: ex.ToString()));
                        return response;
                    }
                    return response;
                })
            });

            //Directory => {0}/delete
            routes.Add(new HularionRoute<DirectoryDeleteRequest, DirectoryDeleteResponse>
            {
                Route = MakeDirectoryRoute("delete"),
                Name = "Delete Directory",
                Method = "DeleteDirectory",
                Usage = "Deleted the specified Directory if it is empty.\n"
                        + "Recursively deletes contents if DeleteContents == true, which may result in only a partial delete if one file or directory cannot be deleted.",
                Handler = ParameterizedFacade.FromSingle<RoutedRequest<DirectoryDeleteRequest>, RoutedResponse<DirectoryDeleteResponse>>(request =>
                {
                    var response = new RoutedResponse<DirectoryDeleteResponse>();
                    try
                    {
                        if (request.Detail.DeleteContents)
                        {
                            var traverser = new TreeTraverser<string>();
                            var plan = traverser.CreateEvaluationPlan(TreeTraversalOrder.LeftRightParent, request.Detail.Directory, node =>
                            {
                                return Directory.GetDirectories(node);
                            }, true);
                            foreach(var directory in plan)
                            {
                                var files = Directory.GetFiles(directory);
                                foreach(var file in files)
                                {
                                    File.Delete(file);
                                }
                                Directory.Delete(directory);
                            }
                        }
                        else
                        {
                            Directory.Delete(request.Detail.Directory);
                        }
                    }
                    catch(Exception ex)
                    {
                        response.SetAsFailure(request.CreateErrorMessage(header: "Exception", message:ex.ToString()));
                        return response;
                    }
                    return response;
                })
            });

            //Directory => {0}/move
            routes.Add(new HularionRoute<DirectoryMoveRequest, DirectoryMoveResponse>
            {
                Route = MakeDirectoryRoute("move"),
                Name = "Move Directory",
                Method = "MoveDirectory",
                Usage = "Moves a Directory and its contents to the DestinationDirectory.",
                Handler = ParameterizedFacade.FromSingle<RoutedRequest<DirectoryMoveRequest>, RoutedResponse<DirectoryMoveResponse>>(request =>
                {
                    var response = new RoutedResponse<DirectoryMoveResponse>();
                    try
                    {
                        Directory.Move(request.Detail.Directory, request.Detail.DestinationDirectory);
                    }
                    catch(Exception ex)
                    {
                        response.SetAsFailure(request.CreateErrorMessage(header: "Exception", message:ex.ToString()));
                    }
                    response.SetAsSuccess();
                    return response;
                })
            });

            //Directory => {0}/read
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

            //Base => {0}/drives
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
            //File => {0}/allAttributes
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

            //File => {0}/attributes
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

            //File => {0}/copy
            routes.Add(new HularionRoute<FileCopyRequest, FileCopyResponse>
            {
                Route = MakeFileRoute("copy"),
                Name = "Copy File",
                Method = "CopyFile",
                Usage = "Add a list of copies (e.g. copies : [ {from:'s1', to:'d1'}, {from:'s2', to:'d2'},...]. \n"
                    + "All files that can be copied will be copied.\n"
                    + "By default, new directories will not be added. set CreateDirectories = true to create necessary directories.",
                Handler = ParameterizedFacade.FromSingle<RoutedRequest<FileCopyRequest>, RoutedResponse<FileCopyResponse>>(request =>
                {
                    var response = new RoutedResponse<FileCopyResponse>();

                    foreach (var copy in request.Detail.Copies)
                    {
                        if (String.IsNullOrWhiteSpace(copy.From) || !copy.From.Contains(@"\"))
                        {
                            response.Detail.Failures.Add(new FileCopyOperationFailure() { Copy = copy, Message = "Invalid Source Filename" });
                        }
                        if (String.IsNullOrWhiteSpace(copy.To) || !copy.To.Contains(@"\"))
                        {
                            response.Detail.Failures.Add(new FileCopyOperationFailure() { Copy = copy, Message = "Invalid Destination Filename" });
                        }

                        var toDirectory = copy.To.Substring(0, copy.To.LastIndexOf(@"\"));

                        if (!File.Exists(copy.From))
                        {
                            response.Detail.Failures.Add(new FileCopyOperationFailure() { Copy = copy, Message = "From File Does Not Exist" });
                            continue;
                        }

                        if (!request.Detail.CreateDirectories && !Directory.Exists(toDirectory))
                        {
                            if (!Directory.Exists(toDirectory))
                            {
                                response.Detail.Failures.Add(new FileCopyOperationFailure() { Copy = copy, Message = "To Directory Does Not Exist and CreateDirectories == false" });
                                continue;
                            }
                        }

                        try
                        {
                            if (request.Detail.CreateDirectories && !Directory.Exists(toDirectory))
                            {
                                Directory.CreateDirectory(toDirectory);
                            }
                            File.Move(copy.From, copy.To);
                        }
                        catch (Exception ex)
                        {
                            response.Detail.Failures.Add(new FileCopyOperationFailure() { Copy = copy, Message = ex.ToString() });
                        }

                        response.Detail.Successes.Add(copy);
                    }
                    return response;
                })
            });

            //File => {0}/delete
            routes.Add(new HularionRoute<FileDeleteRequest, FileDeleteResponse>
            {
                Route = MakeFileRoute("delete"),
                Name = "Delete File",
                Method = "DeleteFile",
                Usage = "Deletes files specified in Filenames.",
                Handler = ParameterizedFacade.FromSingle<RoutedRequest<FileDeleteRequest>, RoutedResponse<FileDeleteResponse>>(request =>
                {
                    var response = new RoutedResponse<FileDeleteResponse>();

                    foreach(var filename in request.Detail.Filenames)
                    {

                        if (String.IsNullOrWhiteSpace(filename) || !File.Exists(filename))
                        {
                            response.Detail.Errors.Add(new FileDeleteError() { Filename = filename, Message="File Does Not Exist" });
                        }

                        try
                        {
                            File.Delete(filename);
                        }
                        catch(Exception ex)
                        {
                            response.Detail.Errors.Add(new FileDeleteError() { Filename = filename, Message = ex.ToString() });
                        }
                        response.Detail.Successes.Add(filename);

                    }

                    response.SetAsFailure(request.CreateErrorMessage(header: "Not Implemented - DirectoryCopyRequest"));
                    return response;
                })
            });

            //File => {0}/move
            routes.Add(new HularionRoute<FileMoveRequest, FileMoveResponse>
            {
                Route = MakeFileRoute("move"),
                Name = "Move Files",
                Method = "MoveFiles",
                Usage = "Add a list of moves (e.g. moves : [ {from:'s1', to:'d1'}, {from:'s2', to:'d2'},...]. \n"
                    + "All files that can be moved will be moved.\n"
                    + "By default, new directories will not be added. set CreateDirectories = true to create necessary directories.",
                Handler = ParameterizedFacade.FromSingle<RoutedRequest<FileMoveRequest>, RoutedResponse<FileMoveResponse>>(request =>
                {
                    var response = new RoutedResponse<FileMoveResponse>();

                    foreach(var move in request.Detail.Moves)
                    {
                        if (String.IsNullOrWhiteSpace(move.From) || !move.From.Contains(@"\"))
                        {
                            response.Detail.Failures.Add(new FileMoveOperationFailure() { Move = move, Message = "Invalid Source Filename" });
                        }
                        if (String.IsNullOrWhiteSpace(move.To) || !move.To.Contains(@"\"))
                        {
                            response.Detail.Failures.Add(new FileMoveOperationFailure() { Move = move, Message = "Invalid Destination Filename" });
                        }

                        var toDirectory = move.To.Substring(0, move.To.LastIndexOf(@"\"));

                        if (!File.Exists(move.From))
                        {
                            response.Detail.Failures.Add(new FileMoveOperationFailure() { Move = move, Message = "From File Does Not Exist" });
                            continue;
                        }

                        if (!request.Detail.CreateDirectories && !Directory.Exists(toDirectory))
                        {
                            if (!Directory.Exists(toDirectory))
                            {
                                response.Detail.Failures.Add(new FileMoveOperationFailure() { Move = move, Message = "To Directory Does Not Exist and CreateDirectories == false" });
                                continue;
                            }
                        }

                        try
                        {
                            if(request.Detail.CreateDirectories && !Directory.Exists(toDirectory))
                            {
                                Directory.CreateDirectory(toDirectory);
                            }
                            File.Move(move.From, move.To);
                        }
                        catch(Exception ex)
                        {
                            response.Detail.Failures.Add(new FileMoveOperationFailure() { Move = move, Message = ex.ToString() });
                        }

                        response.Detail.Successes.Add(move);
                    }
                    return response;
                })
            });

            //File => {0}/set
            routes.Add(new HularionRoute<FileSetRequest, FileSetResponse>
            {
                Route = MakeFileRoute("set"),
                Name = "Set File",
                Method = "SetFile",
                Usage = "Updates the files, creating them if necessary along with the containing directories. If Bytes is not null, Bytes are written. Otherwise, Text is written.",
                Handler = ParameterizedFacade.FromSingle<RoutedRequest<FileSetRequest>, RoutedResponse<FileSetResponse>>(request =>
                {
                    var response = new RoutedResponse<FileSetResponse>();

                    response.Messages.Add(new RoutedResponseMessage() { Message = System.Text.Json.JsonSerializer.Serialize(request) });
                    try
                    {

                        foreach (var set in request.Detail.Sets)
                        {
                            if (String.IsNullOrWhiteSpace(set.Filename) || !set.Filename.Contains(@"\"))
                            {
                                response.Detail.Failures.Add(new FileSetOperationFailure() { Set = set, Message = "Invalid Filename" });
                                continue;
                            }
                            var directory = set.Filename.Substring(0, set.Filename.LastIndexOf(@"\"));
                            try
                            {

                                if (!Directory.Exists(directory))
                                {
                                    Directory.CreateDirectory(directory);
                                }
                                if (set.Bytes != null && set.Bytes.Length > 0)
                                {
                                    File.WriteAllBytes(set.Filename, set.Bytes);
                                }
                                else if (!String.IsNullOrEmpty(set.Text))
                                {
                                    File.WriteAllText(set.Filename, set.Text);
                                    return response;
                                }
                            }
                            catch (Exception ex)
                            {
                                response.Detail.Failures.Add(new FileSetOperationFailure() { Set = set, Message = ex.ToString() });
                            }
                            response.Detail.Successes.Add(set.Filename);
                        }
                    }
                    catch(Exception ex)
                    {
                        response.SetAsFailure(new RoutedResponseMessage(header: "exception", message: ex.ToString()));
                    }
                    return response;
                })
            });

            //File => {0}/read
            routes.Add(new HularionRoute<FileReadRequest, FileReadResponse>
            {
                Route = MakeFileRoute("read"),
                Name = "Read Files",
                Method = "ReadFiles",
                Usage = "Reads the specified files: { Reads: [ {Filename: 'f1', ReadBytes: true},  {Filename: 'f2', ReadText: true}, ...]}\n"
                        + "Reads the whole files and returns either the Bytes array or a Text string.",
                Handler = ParameterizedFacade.FromSingle<RoutedRequest<FileReadRequest>, RoutedResponse<FileReadResponse>>(request =>
                {
                    var response = new RoutedResponse<FileReadResponse>();

                    //curious why we need -->
                    response.Detail = new FileReadResponse();

                    foreach (var read in request.Detail.Reads)
                    {
                        try
                        {
                            if (!File.Exists(read.Filename))
                            {
                                response.Detail.Failures.Add(new FileReadOperationFailure() { Read = read, Message = "File Does Not Exist" });
                            }
                            if ((read.ReadBytes && read.ReadText) || (!read.ReadBytes && !read.ReadText))
                            {
                                response.Detail.Failures.Add(new FileReadOperationFailure() { Read = read, Message = "Exactly one of ReadBytes and ReadText must be true." });
                            }
                            var result = new FileReadOperationResult();
                            if (read.ReadBytes)
                            {
                                result.Bytes = File.ReadAllBytes(read.Filename);
                                response.Detail.Successes.Add(result);
                                continue;
                            }
                            if (read.ReadText)
                            {
                                result.Text = File.ReadAllText(read.Filename);
                                response.Detail.Successes.Add(result);
                                continue;
                            }
                        }
                        catch (Exception ex)
                        {
                            response.Detail.Failures.Add(new FileReadOperationFailure() { Read = read, Message = ex.ToString() });
                        }
                    }
                    return response;
                })
            });

        }

        private void AddSystemRoutes(List<HularionRoute> routes)
        {
            //System => {0}/openfolder
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
