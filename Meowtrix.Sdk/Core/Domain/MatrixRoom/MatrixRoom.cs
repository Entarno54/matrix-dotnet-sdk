using System;
using System.Collections.Generic;
using Meowtrix.Sdk.Core.Domain.MatrixRoom;
using Meowtrix.Sdk.Core.Domain.RoomEvent;
using Meowtrix.Sdk.Core.Infrastructure.Dto.Room.Create;
using Meowtrix.Sdk.Core.Infrastructure.Dto.Sync;
using Meowtrix.Sdk.Core.Infrastructure.Dto.Sync.Event;
using Meowtrix.Sdk.Core.Infrastructure.Dto.Sync.Event.Room;

namespace Meowtrix.Sdk.Core.Domain.MatrixRoom;

    public class MatrixRoom
    {
        public readonly string Id ;
        public readonly MatrixRoomStatus Status ;
        public readonly List<string> JoinedUserIds ;
        public RoomType RoomType ;
        public List<MatrixRoom> Children = new();

        public MatrixRoom(string roomId, MatrixRoomStatus status, List<string> joinedUserIds, RoomType roomType)
        {
            Id = roomId;
            Status = status;
            JoinedUserIds = joinedUserIds;
            RoomType = roomType;
        }
        
        public static MatrixRoom Create(string roomId, RoomResponse joinedRoom, MatrixRoomStatus status)
        {
            var joinedUserIds = new List<string>();
            foreach (RoomEventResponse timelineEvent in joinedRoom.Timeline.Events)
            {
                if (MembershipEvent.Factory.TryCreateFrom(timelineEvent, roomId, out MembershipEvent joinRoomEvent))
                    joinedUserIds.Add(joinRoomEvent!.SenderUserId);
                Console.WriteLine(timelineEvent.EventType);
            }

            return new MatrixRoom(roomId, status, joinedUserIds, RoomType.Room);
        }
        
        
        public static MatrixRoom CreateInvite(string roomId, InvitedRoom invitedRoom)
        {
            var joinedUserIds = new List<string>();
            foreach (RoomStrippedState timelineEvent in invitedRoom.InviteState.Events)
                if (MembershipEvent.Factory.TryCreateFromStrippedState(timelineEvent, roomId,
                        out MembershipEvent joinRoomEvent))
                    joinedUserIds.Add(joinRoomEvent!.SenderUserId);

            return new MatrixRoom(roomId, MatrixRoomStatus.Invited, joinedUserIds, RoomType.Room);
        }
    }
    
    
