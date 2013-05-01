% Programmer:  James L. Armes
%
% Purpose:  Plots mesh generated by numeric model
function [] = PlotMesh(fileName)
figure(1); hold on;
M = CSVREAD(fileName, 1, 0);

x = M(:,2);
y = M(:,3);
mat = M(:,4);

for i = 1:max(size(x))
   % Copper
    if mat(i) == 1
       plot (x(i),y(i), 'or');
    end
   
    % BiTE
   if mat(i) == 2
       plot (x(i),y(i), '^k');
   end
   
   % Ceramic
   if mat(i) == 3
       plot (x(i),y(i), '*y');
   end
   
   % Air
   if mat(i) == 4
       plot(x(i),y(i),'+');
   end
   
   
end

%plot(x,y, '*k'); 
xlabel('X Position, m'); ylabel('Y Position, m');


end