cd C:\Users\Admin\Documents\GitHub\TeamDaddy2.2\Web\client-server proto

python -m grpc_tools.protoc -I. --python_out=. --grpc_python_out=. website.proto
