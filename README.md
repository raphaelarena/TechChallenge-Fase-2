# TechChallenge - Fase 2

Função Orquestradora de Aprovação de Pedidos
Introdução

A função PedidoApprovalFunction é uma orquestração de fluxo de trabalho que coordena e controla o processo de aprovação de pedidos em uma aplicação de e-commerce, aplicando lógicas específicas em diferentes etapas do processo.

Metodos:

1 - PedidoHttpStart: Este é o ponto de entrada HTTP da função. Quando um pedido é feito através desta função, uma nova instância do PedidoApprovalFunction é iniciada, passando o pedido como entrada.

2 - SubmeterPedido: Uma atividade que faz a submissão de um pedido de compra. Neste passo, o pedido é avaliado com base em seu valor total. Se o valor estiver abaixo de R$ 1000, o pedido é marcado como "Aprovado" caso contrário é marcado como "Rejeitado".

3 - PrimeiraAprovacao: Uma atividade que faz a primeira etapa de aprovação do pedido. O pedido é novamente avaliado com base em seu valor total. Se o valor estiver abaixo de R$ 500, é "Aprovado" entre R$ 500 e R$ 1000 é "Aprovado com observação" acima de R$ 1000, é "Rejeitado".

4 - SegundaAprovacao: Uma atividade que faz a segunda etapa de aprovação do pedido. Nesta fase, o pedido é avaliado com base na quantidade. Se o pedido tiver até 10 itens, é "Aprovado"; entre 10 e 20 itens, é "Aprovado com observação" mais de 20 itens, é "Rejeitado".

5 - PedidoApprovalFunction: Esta é a função principal. Ela coordena o fluxo de trabalho, chamando as atividades SubmeterPedido, PrimeiraAprovacao e SegundaAprovacao em paralelo. Após receber os resultados de todas as atividades, ela toma uma decisão final sobre o pedido:

Se qualquer uma das etapas indicar "Rejeitado", o pedido é marcado como "Rejeitado".
Se todas as etapas indicarem "Aprovado", o pedido é marcado como "Aprovado".
Se alguma etapa indicar "Aprovado com observação", o pedido é marcado como "Aprovado com observação".

Fluxo

1 - Um pedido é feito via HTTP POST para a função PedidoHttpStart, passando os detalhes do pedido.

2 - A função PedidoHttpStart inicia uma nova instância da PedidoApprovalFunction, passando o objeto Pedido.

3 - O PedidoApprovalFunction inicia as atividades SubmeterPedido, PrimeiraAprovacao e SegundaAprovacao em paralelo.

4 - Cada atividade avalia o pedido com base em critérios específicos como valor total, quantidade de itens e depois disso retorna um resultado.

5 - O PedidoApprovalFunction recebe os resultados das atividades e toma uma decisão final sobre o status do pedido.

6 - O resultado final é retornado como resposta ao cliente que submeteu o pedido.
